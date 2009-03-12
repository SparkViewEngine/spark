
#include "stdafx.h"
#include "Source.h"

STDMETHODIMP Source::SetSupervisor(ISourceSupervisor* pSupervisor) 
{
	if (_supervisorAdvise)
	{
		if (_supervisor != NULL)
			_supervisor->Unadvise(_supervisorAdvise);
		_supervisorAdvise = 0;
	}

	_supervisor = pSupervisor;
	if (_supervisor != NULL)
		_supervisor->Advise(this, &_supervisorAdvise);

	return S_OK;
}

STDMETHODIMP Source::GetRunningDocumentText(BSTR CanonicalName, BSTR *pText)
{
	*pText = NULL;

	HRESULT hr = S_OK;
	CComPtr<IVsRunningDocumentTable> runningDocumentTable;
	_HR(_site->QueryService(__uuidof(IVsRunningDocumentTable), &runningDocumentTable));
	
	CComPtr<IUnknown> punkDocument;
	if (SUCCEEDED(hr))
	{
		CComPtr<IVsHierarchy> hierarchy;
		VSITEMID itemid = 0;
		VSCOOKIE cookie = 0;
		HRESULT hrFind = runningDocumentTable->FindAndLockDocument(0, CanonicalName, &hierarchy, &itemid, &punkDocument, &cookie);
		if (FAILED(hrFind) || itemid == VSITEMID_NIL)
		{
			// return (string)null
			*pText = NULL;
			return S_OK;
		}
	}

	CComPtr<IVsTextLines> textLines;	
	_HR(punkDocument->QueryInterface(&textLines));

	long iLine = 0;
	long iIndex = 0;
	_HR(textLines->GetLastLineIndex(&iLine, &iIndex));
	_HR(textLines->GetLineText(0, 0, iLine, iIndex, pText));

	return hr;
}

STDMETHODIMP Source::GetPaint( 
    /* [out] */ long *cPaint,
    /* [size_is][size_is][out] */ SourcePainting **prgPaint)
{
	HRESULT hr = S_OK;
	*cPaint = _paintLength;
	*prgPaint = new SourcePainting[_paintLength];
	CopyMemory(*prgPaint, _paintArray, sizeof(SourcePainting) * _paintLength);
	return hr;
}


HRESULT SiteObject(IUnknown* obj, IServiceProvider* site)
{
	CComQIPtr<IObjectWithSite> ows(obj);
	return (ows == NULL) ? S_OK : ows->SetSite(site);
}

HRESULT Source::FinalConstruct()
{
	HRESULT hr = S_OK;

	CComPtr<ILocalRegistry> reg;
	_HR(_site->QueryService(__uuidof(ILocalRegistry), &reg));


	// Initialize secondary buffer and coordinator
	_HR(reg->CreateInstance(__uuidof(VsTextBuffer), NULL, __uuidof(IVsTextLines), CLSCTX_INPROC_SERVER, (void**)&_secondaryBuffer));
	_HR(SiteObject(_secondaryBuffer, _site));
	_HR(_secondaryBuffer->SetLanguageServiceID(__uuidof(CSharp)));

	_HR(reg->CreateInstance(__uuidof(VsTextBufferCoordinator), NULL, __uuidof(IVsTextBufferCoordinator), CLSCTX_INPROC_SERVER, (void**)&_bufferCoordinator));
	_HR(SiteObject(_bufferCoordinator, _site));
	_HR(_bufferCoordinator->SetBuffers(_primaryBuffer, _secondaryBuffer));

	// Get the moniker for the text buffer
	CComPtr<IVsUserData> userData;
	_HR(_primaryBuffer->QueryInterface(&userData));		
	CComVariant moniker;
	_HR(userData->GetData(__uuidof(IVsUserData), &moniker));
	_HR(moniker.ChangeType(VT_BSTR));

	// Locate hierarchy itemid
	CComPtr<IWebApplicationCtxSvc> webApplicationCtx;
	_HR(_site->QueryService(__uuidof(IWebApplicationCtxSvc), &webApplicationCtx));
	_HR(webApplicationCtx->GetItemContextFromPath(V_BSTR(&moniker), FALSE, &_hierarchy, &_itemid));

	// Locate intellisense project manager
	CComPtr<IServiceProvider> itemServices;
	_HR(webApplicationCtx->GetItemContext(_hierarchy, _itemid, &itemServices));		
	_HR(itemServices->QueryService(__uuidof(SVsIntellisenseProjectManager), &_projectManager));

	// Initialize contained language
	CComPtr<IVsContainedLanguageFactory> containedLanguagefactory;
	_HR(_projectManager->GetContainedLanguageFactory(CComBSTR(_T("CSharp")), &containedLanguagefactory));
	_HR(containedLanguagefactory->GetLanguage(_hierarchy, _itemid, _bufferCoordinator, &_containedLanguage));
	_HR(_containedLanguage->SetHost(this));
	
	return hr;
}

STDMETHODIMP Source::GetDefaultPageBaseType(BSTR* pPageBaseType)
{
	CComBSTR pageBaseType;

	HRESULT hr = S_OK;
	CComVariant varProject;
	_HR(_hierarchy->GetProperty(VSITEMID_ROOT, VSHPROPID_ExtObject, &varProject));
	_HR(varProject.ChangeType(VT_UNKNOWN));
	
	CComPtr<DTE_Project> dteProject;
	_HR(V_UNKNOWN(&varProject)->QueryInterface(&dteProject));

	CComPtr<IDispatch> dispProject;
	_HR(dteProject->get_Object(&dispProject));

	CComPtr<VSProject> vsProject;
	_HR(dispProject->QueryInterface(&vsProject));

	CComPtr<References> references;
	_HR(vsProject->get_References(&references));

	CComPtr<IUnknown> punkEnum;
	_HR(references->_NewEnum(&punkEnum));

	CComPtr<IEnumVARIANT> pvarEnum;
	_HR(punkEnum->QueryInterface(&pvarEnum));

	while(SUCCEEDED(hr))
	{
		CComVariant varReference;
		ULONG cFetched = 0;
		HRESULT hrEnum = pvarEnum->Next(1, &varReference, &cFetched);
		if (hrEnum != S_OK || cFetched == 0)
			break;

		_HR(varReference.ChangeType(VT_UNKNOWN));

		CComPtr<Reference> reference;
		_HR(V_UNKNOWN(&varReference)->QueryInterface(&reference));

		CComBSTR name;
		_HR(reference->get_Name(&name));

		if (CompareStringW(LOCALE_INVARIANT, NORM_IGNORECASE, name, -1, L"Spark.Web.Mvc", -1) == CSTR_EQUAL)
		{
			pageBaseType = L"Spark.Web.Mvc.SparkView";
			break;
		}
		else if (CompareStringW(LOCALE_INVARIANT, NORM_IGNORECASE, name, -1, L"Castle.MonoRail.Views.Spark", -1) == CSTR_EQUAL)
		{
			pageBaseType = L"Castle.MonoRail.Views.Spark.SparkView";
			break;
		}
	}

	*pPageBaseType = pageBaseType.Detach();
	return hr;
}


STDMETHODIMP Source::EnsureSecondaryBufferReady()
{
	HRESULT hr = S_OK;

	long iLastLine = 0;
	long iLastIndex = 0;
	_HR(_primaryBuffer->GetLastLineIndex(&iLastLine, &iLastIndex));

	CComBSTR primaryText;
	_HR(_primaryBuffer->GetLineText(0, 0, iLastLine, iLastIndex, &primaryText));

	// primary text has not changed - do nothing
	if (FAILED(hr) || primaryText == _primaryText)
		return hr;

	_primaryText.Attach(primaryText.Detach());

	_HR(_supervisor->PrimaryTextChanged(TRUE));

	return hr;
}

STDMETHODIMP Source::OnGenerated( 
    /* [in] */ BSTR primaryText,
    /* [in] */ BSTR secondaryText,
    /* [in] */ long cMappings,
    /* [size_is][in] */ SourceMapping *rgSpans,
	/* [in] */ long cPaints,
	/* [size_is][in] */ SourcePainting *rgPaints)
{
	HRESULT hr = S_OK;

	long iReplaceLastLine = 0;
	long iReplaceLastIndex = 0;
	_HR(_secondaryBuffer->GetLastLineIndex(&iReplaceLastLine, &iReplaceLastIndex));
	CComBSTR existingSecondaryText;
	_HR(_secondaryBuffer->GetLineText(0, 0, iReplaceLastLine, iReplaceLastIndex, &existingSecondaryText));

	// secondary text already current - assume buffer coordinator worked and do nothing
	if (FAILED(hr) || secondaryText == existingSecondaryText)
		return hr;

	TextSpan changedSpan = {0};
	_HR(_secondaryBuffer->ReplaceLines(
		0, 0, iReplaceLastLine, iReplaceLastIndex, 
		secondaryText, SysStringLen(secondaryText), 
		&changedSpan));

	if (cMappings != 0)
	{
		NewSpanMapping* mappings = new NewSpanMapping[cMappings];
		ZeroMemory(mappings, sizeof(NewSpanMapping) * cMappings);
		for(int index = 0; index != cMappings; ++index)
		{
			_primaryBuffer->GetLineIndexOfPosition(
				rgSpans[index].start1,
				&mappings[index].tspSpans.span1.iStartLine,
				&mappings[index].tspSpans.span1.iStartIndex);
			_primaryBuffer->GetLineIndexOfPosition(
				rgSpans[index].end1,
				&mappings[index].tspSpans.span1.iEndLine,
				&mappings[index].tspSpans.span1.iEndIndex);
			_secondaryBuffer->GetLineIndexOfPosition(
				rgSpans[index].start2,
				&mappings[index].tspSpans.span2.iStartLine,
				&mappings[index].tspSpans.span2.iStartIndex);
			_secondaryBuffer->GetLineIndexOfPosition(
				rgSpans[index].end2,
				&mappings[index].tspSpans.span2.iEndLine,
				&mappings[index].tspSpans.span2.iEndIndex);
		}

		_HR(_bufferCoordinator->SetSpanMappings(cMappings, mappings));
		delete[cMappings] mappings;
	}

	if (_paintArray != NULL)
	{
		delete[_paintLength] _paintArray;
		_paintArray = NULL;
	}

	_paintLength = cPaints;
	_paintArray = new SourcePainting[_paintLength];
	CopyMemory(_paintArray, rgPaints, cPaints * sizeof(SourcePainting));

	return hr;
}

STDMETHODIMP Source::GetLineIndent( 
	/* [in] */ long lLineNumber,
	/* [out] */ __RPC__deref_out_opt BSTR *pbstrIndentString,
	/* [out] */ __RPC__out long *plParentIndentLevel,
	/* [out] */ __RPC__out long *plIndentSize,
	/* [out] */ __RPC__out BOOL *pfTabs,
	/* [out] */ __RPC__out long *plTabSize) 
{
	*pbstrIndentString = SysAllocString(L"  ");
	*plParentIndentLevel = 0;
	*plIndentSize = 2;
	*pfTabs = FALSE;
	*plTabSize = 4;
	return S_OK;
}

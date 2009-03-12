
#include "stdafx.h"
#include "TextViewFilter.h"

HRESULT TextViewFilter::FinalConstruct()
{
	HRESULT hr = S_OK;

	// get references to existing source, buffer coordinator, and contained language instances
	CComPtr<IVsTextLines> textLines;
	_HR(_textView->GetBuffer(&textLines));
	_HR(_language->GetSource(textLines, &_source));

	CComPtr<IVsTextBufferCoordinator> bufferCoordinator;
	_HR(_source->GetTextBufferCoordinator(&bufferCoordinator));

	CComPtr<IVsContainedLanguage> containedLanguage;
	_HR(_source->GetContainedLanguage(&containedLanguage));


	// Create intellisense host from text view's host provider
	CComPtr<IVsTextViewIntellisenseHostProvider> intellisenseHostProvider;
	_HR(_textView->QueryInterface(&intellisenseHostProvider));
	_HR(intellisenseHostProvider->CreateIntellisenseHost(bufferCoordinator, __uuidof(_intellisenseHost), (void**)&_intellisenseHost));


	// Obtain the contained text view filter and command target
	_HR(containedLanguage->GetTextViewFilter(_intellisenseHost, this, &_containedTextViewFilter));
	_HR(_containedTextViewFilter->QueryInterface(&_containedCommandTarget));

	// Wedge command target and text view filter into chain
	_HR(_textView->AddCommandFilter(_containedCommandTarget, &_nextCommandTarget));

	// Hold onto the targets that are chained after the ones we've added
	_chainCommandTarget = _nextCommandTarget;
	_chainTextViewFilter = _nextCommandTarget;
	
	CComPtr<IVsIntellisenseProjectManager> projectManager;
	_HR(_source->GetIntellisenseProjectManager(&projectManager));
	_HR(projectManager->OnEditorReady());
	_HR(projectManager->CompleteIntellisenseProjectLoad());	
	return hr;
}

STDMETHODIMP TextViewFilter::QueryStatus( 
    /* [unique][in] */ __RPC__in_opt const GUID *pguidCmdGroup,
    /* [in] */ ULONG cCmds,
    /* [out][in][size_is] */ __RPC__inout_ecount_full(cCmds) OLECMD prgCmds[  ],
    /* [unique][out][in] */ __RPC__inout_opt OLECMDTEXT *pCmdText)
{
	if (_chainCommandTarget == NULL)
		return S_OK;

	return _chainCommandTarget->QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
}


class __declspec(uuid("1496A755-94DE-11D0-8C3F-00C04FC2AAE2")) StandardCommandSet2K;

STDMETHODIMP TextViewFilter::Exec( 
    /* [unique][in] */ __RPC__in_opt const GUID *pguidCmdGroup,
    /* [in] */ DWORD nCmdID,
    /* [in] */ DWORD nCmdexecopt,
    /* [unique][in] */ __RPC__in_opt VARIANT *pvaIn,
    /* [unique][out][in] */ __RPC__inout_opt VARIANT *pvaOut)
{
	if (_chainCommandTarget == NULL)
		return S_OK;

	HRESULT hr = S_OK;

	if (*pguidCmdGroup == __uuidof(StandardCommandSet2K))
	{
		switch(nCmdID)
		{
		case ECMD_TYPECHAR:
			{				
				CComVariant varIn;
				_HR(varIn.ChangeType(VT_UI2, pvaIn));
				CComPtr<ISourceSupervisor> supervisor;
				_HR(_source->GetSupervisor(&supervisor));
				CComBSTR key(1, (LPCOLESTR)&V_UI2(&varIn));
//				ATLTRACE(TEXTVIEWFILTER_EXEC, 4, L"ECMD_TYPECHAR '%s'", key.m_str);
				_HR(supervisor->OnTypeChar(_textView, key));
			}
			break;
		}

	}
	_HR(_chainCommandTarget->Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut));
	return hr;
}

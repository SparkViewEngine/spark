
#include "stdafx.h"
#include "Language.h"
#include "CodeWindowManager.h"
#include "Colorizer.h"
#include "Source.h"
#include "ColorableItem.h"


STDMETHODIMP Language::GetSource(IVsTextBuffer* pBuffer, ISparkSource** ppSource)
{
	HRESULT hr = S_OK;

	CComCritSecLock<CComCriticalSection> lock(_sourcesLock);

	CComPtr<IUnknown> key;
	_HR(pBuffer->QueryInterface(&key));

	int index = _sources.FindKey(key);

	if (index == -1)
	{
		CComPtr<ISparkSource> source;
		
		SourceInit init = {_site};
		_HR(pBuffer->QueryInterface(&init._primaryBuffer));
		_HR(Source::CreateInstance(init, &source));
		
		_HR(_supervisor->OnSourceAssociated(source));

		_HR(source->QueryInterface(ppSource));

		_sources.Add(key.Detach(), source.Detach());
	}
	else
	{
		_sources.GetValueAt(index)->QueryInterface(ppSource);
	}
	return hr;
}

STDMETHODIMP Language::GetColorizer( 
    /* [in] */ __RPC__in_opt IVsTextLines *pBuffer,
    /* [out] */ __RPC__deref_out_opt IVsColorizer **ppColorizer)
{
	HRESULT hr = S_OK;

	CComPtr<IVsProvideColorableItems> csharpItems;
	_HR(_site->QueryService(__uuidof(CSharp), &csharpItems));
	int csharpItemCount;
	_HR(csharpItems->GetItemCount(&csharpItemCount));

	ColorizerInit init = {this, pBuffer, csharpItemCount + 1};
	_HR(Colorizer::CreateInstance(init, ppColorizer));
	return hr;
}

STDMETHODIMP Language::GetCodeWindowManager( 
    /* [in] */ __RPC__in_opt IVsCodeWindow *pCodeWin,
    /* [out] */ __RPC__deref_out_opt IVsCodeWindowManager **ppCodeWinMgr)
{
	HRESULT hr = S_OK;
	CodeWindowManagerInit init = {this, pCodeWin};
	_HR(CodeWindowManager::CreateInstance(init, ppCodeWinMgr));
	return hr;
}

ColorableItemInit g_colors[] = 
{
	{L"Spark Default", 
		CI_YELLOW, CI_BLACK, FF_DEFAULT},
	{L"Spark Attribute Name", 
		CI_RED, CI_WHITE, FF_DEFAULT},
	{L"Spark Attribute Quotes", 
		CI_BLACK, CI_WHITE, FF_DEFAULT},
	{L"Spark Attribute Value", 
		CI_BLUE, CI_WHITE, FF_DEFAULT},
	{L"Spark CDATA", 
		CI_DARKGRAY, CI_WHITE, FF_DEFAULT},
	{L"Spark Comment", 
		CI_DARKGREEN, CI_WHITE, FF_DEFAULT},
	{L"Spark Delimiter", 
		CI_BLUE, CI_WHITE, FF_DEFAULT},
	{L"Spark Keyword", 
		CI_BLACK, CI_CYAN, FF_DEFAULT},
	{L"Spark Code", 
		CI_MAROON, CI_WHITE, FF_DEFAULT},
	{L"Spark Element Name", 
		CI_MAROON, CI_WHITE, FF_DEFAULT},
	{L"Spark Text", 
		CI_BLACK, CI_WHITE, FF_DEFAULT},
	{L"Spark Processing Instruction", 
		CI_BLACK, CI_CYAN, FF_DEFAULT},
	{L"Spark String", 
		CI_BLACK, CI_CYAN, FF_DEFAULT},
};
    //SPARKPAINT_Default,
    //SPARKPAINT_AttributeName,
    //SPARKPAINT_AttributeQuotes,
    //SPARKPAINT_AttributeValue,
    //SPARKPAINT_CDATASection,
    //SPARKPAINT_Comment,
    //SPARKPAINT_Delimiter,
    //SPARKPAINT_Keyword,
    //SPARKPAINT_Code,
    //SPARKPAINT_ElementName,
    //SPARKPAINT_Text,
    //SPARKPAINT_ProcessingInstruction,
    //SPARKPAINT_String

STDMETHODIMP Language::GetItemCount( 
    /* [out] */ __RPC__out int *piCount)
{
	HRESULT hr = S_OK;

	CComPtr<IVsProvideColorableItems> csharpItems;
	_HR(_site->QueryService(__uuidof(CSharp), &csharpItems));
	int csharpItemCount;
	_HR(csharpItems->GetItemCount(&csharpItemCount));
	
	*piCount = csharpItemCount + ARRAYSIZE(g_colors);
	return hr;
}

STDMETHODIMP Language::GetColorableItem( 
    /* [in] */ int iIndex,
    /* [out] */ __RPC__deref_out_opt IVsColorableItem **ppItem)
{
	HRESULT hr = S_OK;

	CComPtr<IVsProvideColorableItems> csharpItems;
	_HR(_site->QueryService(__uuidof(CSharp), &csharpItems));
	int csharpItemCount;
	_HR(csharpItems->GetItemCount(&csharpItemCount));

	if (iIndex <= csharpItemCount)
		return csharpItems->GetColorableItem(iIndex, ppItem);

	return ColorableItem::CreateInstance(g_colors[iIndex - csharpItemCount - 1], ppItem);
}

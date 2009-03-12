
#include "stdafx.h"
#include "CodeWindowManager.h"
#include "TextViewFilter.h"

STDMETHODIMP CodeWindowManager::AddAdornments()
{
	HRESULT hr = S_OK;

	if (SUCCEEDED(hr))
	{
		CComPtr<IVsTextView> primaryView;
		HRESULT hrPrimary = _codeWindow->GetPrimaryView(&primaryView);
		if (SUCCEEDED(hrPrimary))
			hr = OnNewView(primaryView);
	}

	if (SUCCEEDED(hr))
	{
		CComPtr<IVsTextView> secondaryView;
		HRESULT hrSecondary = _codeWindow->GetSecondaryView(&secondaryView);
		if (SUCCEEDED(hrSecondary))
			hr = OnNewView(secondaryView);
	}

	return hr;
}

STDMETHODIMP CodeWindowManager::RemoveAdornments()
{	
	CComCritSecLock<CComCriticalSection> lock(_filtersLock);
	for(int index = 0; index != _filters.GetSize(); ++index)
		_filters[index]->Release();
	_filters.RemoveAll();

	return S_OK;
}

STDMETHODIMP CodeWindowManager::OnNewView(IVsTextView *pView)
{
	HRESULT hr = S_OK;

	CComPtr<IUnknown> filter;
	TextViewFilterInit init = {_language, pView};
	_HR(TextViewFilter::CreateInstance(init, &filter));
	
	if (SUCCEEDED(hr))
	{
		CComCritSecLock<CComCriticalSection> lock(_filtersLock);
		_filters.Add(filter.Detach());
	}
	return hr;
}



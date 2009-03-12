
#include "stdafx.h"
#include "Colorizer.h"

HRESULT Colorizer::FinalConstruct()
{
	HRESULT hr = S_OK;
	_HR(_language->GetSource(_buffer, &_source));

	CComPtr<IVsContainedLanguage> containedLanguage;
	_HR(_source->GetContainedLanguage(&containedLanguage));
	CComPtr<IVsColorizer> colorizer;
	_HR(containedLanguage->GetColorizer(&colorizer));
	_HR(colorizer->QueryInterface(&_containedColorizer));
	return hr;
}

STDMETHODIMP Colorizer::BeginColorization()
{
	HRESULT hr = S_OK;
	CComPtr<IVsContainedLanguageHost> host;
	_HR(_source->QueryInterface(&host));
	_HR(host->EnsureSecondaryBufferReady());

	if (_paintArray != NULL)
	{
		delete[_paintLength] _paintArray;
		_paintLength = 0;
		_paintArray = NULL;
	}

	_HR(_source->GetPaint(&_paintLength, &_paintArray));

	return hr;
}

STDMETHODIMP_(long) Colorizer::ColorizeLine( 
    /* [in] */ long iLine,
    /* [in] */ long iLength,
    /* [in] */ __RPC__in const WCHAR *pszText,
    /* [in] */ long iState,
    /* [out] */ __RPC__out ULONG *pAttributes)
{	
	for (long index = 0; index != iLength + 1; ++index)
		pAttributes[index] = 0;

	HRESULT hr = S_OK;
	CComPtr<IVsTextBufferCoordinator> coordinator;
	_HR(_source->GetTextBufferCoordinator(&coordinator));

	CComPtr<IVsTextLines> primary;
	_HR(coordinator->GetPrimaryBuffer(&primary));

	long iLineStart = 0;
	_HR(primary->GetPositionOfLineIndex(iLine, 0, &iLineStart));
	long iLineEnd = iLineStart + iLength;

	for (long index = 0; index != _paintLength; ++index)
	{
		long iColorStart = iLineStart;
		long iColorEnd = iLineEnd;

		if (_paintArray[index].start >= iLineEnd)
		{
			continue;
		}
		else if (_paintArray[index].start >= iLineStart)
		{
			iColorStart = _paintArray[index].start;
		}

		if (_paintArray[index].end <= iLineStart)
		{
			continue;
		}
		else if (_paintArray[index].end <= iLineEnd)
		{
			iColorEnd = _paintArray[index].end;
		}

		for(long iIndex = iColorStart - iLineStart; iIndex != iColorEnd - iLineStart; ++iIndex)
		{
			// one last safety check - just because memory over-runs are so deadly
			if (iIndex >= 0 && iIndex < iLength && _paintArray[index].color != 0)
			{
				pAttributes[iIndex] = _paintArray[index].color + _containedLanguageColorCount;
			}
		}
	}

	CComPtr<IVsEnumBufferCoordinatorSpans> pEnum;
	_HR(coordinator->EnumSpans(&pEnum));
	while(SUCCEEDED(hr))
	{
		NewSpanMapping mapping = {0};
		ULONG cFetched = 0;
		HRESULT hrNext = pEnum->Next(1, &mapping, &cFetched);
		if (hrNext != S_OK || cFetched == 0)
			break;

		if (mapping.tspSpans.span1.iStartLine > iLine)
			continue;
		if (mapping.tspSpans.span1.iEndLine < iLine)
			continue;
		long iFirstIndex = 0;
		long iLastIndex = iLength;
		if (mapping.tspSpans.span1.iStartLine == iLine)
			iFirstIndex = mapping.tspSpans.span1.iStartIndex;
		if (mapping.tspSpans.span1.iEndLine == iLine)
			iLastIndex = mapping.tspSpans.span1.iEndIndex;
		
		long ignore = 0;
		_containedColorizer->ColorizeLineFragment(iLine, iFirstIndex, iLastIndex - iFirstIndex, pszText, 0, pAttributes, &ignore);
	}

	return 0;
}


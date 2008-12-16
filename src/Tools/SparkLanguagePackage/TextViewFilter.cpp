
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


	// Wedge this text view filter into chain
	_HR(_textView->AddCommandFilter(this, &_nextCommandTarget));

	// and obtain the contained text view filter and command target
	_HR(containedLanguage->GetTextViewFilter(_intellisenseHost, _nextCommandTarget, &_containedTextViewFilter));
	_HR(_containedTextViewFilter->QueryInterface(&_containedCommandTarget));	


	CComPtr<IVsIntellisenseProjectManager> projectManager;
	_HR(_source->GetIntellisenseProjectManager(&projectManager));
	_HR(projectManager->OnEditorReady());
	_HR(projectManager->CompleteIntellisenseProjectLoad());


	return hr;
}

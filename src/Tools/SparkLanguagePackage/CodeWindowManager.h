
#pragma once

#include "atlutil.h"
#include "SparkLanguagePackage_i.h"


struct CodeWindowManagerInit
{
	CComPtr<ISparkLanguage> _language;
	CComPtr<IVsCodeWindow> _codeWindow;
};

class ATL_NO_VTABLE CodeWindowManager:
	public CComCreatableObject<CodeWindowManager, CodeWindowManagerInit>,
	public IVsCodeWindowManager
{
	CSimpleArray<IUnknown*> _filters;
	CComAutoCriticalSection _filtersLock;

public:
	BEGIN_COM_MAP(CodeWindowManager)
		COM_INTERFACE_ENTRY(IVsCodeWindowManager)
	END_COM_MAP()
	
	void FinalRelease() 
	{
		RemoveAdornments(); 
	}

	STDMETHODIMP AddAdornments();
    STDMETHODIMP RemoveAdornments();
    STDMETHODIMP OnNewView(IVsTextView *pView);
    
};



#pragma once
#include "atlutil.h"
#include "SparkLanguagePackage_i.h"

class LanguageInit
{
public:
	CComPtr<IServiceProvider> _site;
};

class ATL_NO_VTABLE Language : 
	public CComCreatableObject<Language, LanguageInit>,
	public ISparkLanguage,
	public IVsLanguageInfo,
	public IVsProvideColorableItems
{
	CComAutoCriticalSection _sourcesLock;
	CSimpleMap<IUnknown*, IUnknown*> _sources;
	CComPtr<ILanguageSupervisor> _supervisor;

public:
	Language()
	{
	}

	BEGIN_COM_MAP(Language)
		COM_INTERFACE_ENTRY(ISparkLanguage)
		COM_INTERFACE_ENTRY(IVsLanguageInfo)
		COM_INTERFACE_ENTRY(IVsProvideColorableItems)
	END_COM_MAP()

	/********** ISparkLanguage **********/
	STDMETHODIMP GetSupervisor(ILanguageSupervisor** ppSupervisor) 	{return _supervisor == NULL ? *ppSupervisor = NULL, S_OK : _supervisor->QueryInterface(ppSupervisor);}
	STDMETHODIMP SetSupervisor(ILanguageSupervisor* pSupervisor) {_supervisor = pSupervisor; return S_OK;}
	STDMETHODIMP GetSource(IVsTextBuffer* pBuffer, ISparkSource** ppSource);

	/********** IVsLanguageInfo **********/
    STDMETHODIMP GetLanguageName( 
        /* [out] */ __RPC__deref_out_opt BSTR *bstrName)
	{
		*bstrName = SysAllocString(L"Spark");
		return S_OK;
	}
    
    STDMETHODIMP GetFileExtensions( 
        /* [out] */ __RPC__deref_out_opt BSTR *pbstrExtensions)
	{
		*pbstrExtensions = SysAllocString(L".spark");
		return S_OK;
	}
    
    STDMETHODIMP GetColorizer( 
        /* [in] */ __RPC__in_opt IVsTextLines *pBuffer,
        /* [out] */ __RPC__deref_out_opt IVsColorizer **ppColorizer);
    
    STDMETHODIMP GetCodeWindowManager( 
        /* [in] */ __RPC__in_opt IVsCodeWindow *pCodeWin,
        /* [out] */ __RPC__deref_out_opt IVsCodeWindowManager **ppCodeWinMgr);


	/********** IVsProvideColorableItems **********/
    STDMETHODIMP GetItemCount( 
        /* [out] */ __RPC__out int *piCount);
    
    STDMETHODIMP GetColorableItem( 
        /* [in] */ int iIndex,
        /* [out] */ __RPC__deref_out_opt IVsColorableItem **ppItem);
};


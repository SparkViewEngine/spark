// Package.h : Declaration of the Package

#pragma once
#include "resource.h"       // main symbols

#include "SparkLanguagePackage_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// Package

class ATL_NO_VTABLE Package :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<Package, &CLSID_Package>,
	public ISparkPackage,
	public IVsPackage,
	public IServiceProvider,
	public IVsInstalledProduct
{
	CComPtr<IServiceProvider> _site;
	CComPtr<ISparkLanguage> _language;
	DWORD _dwProfferCookie;

public:
	Package()
	{
		m_pUnkMarshaler = NULL;
		_dwProfferCookie = 0;
	}

DECLARE_REGISTRY_RESOURCEID(IDR_PACKAGE)


BEGIN_COM_MAP(Package)
	COM_INTERFACE_ENTRY(ISparkPackage)
	COM_INTERFACE_ENTRY(IVsPackage)
	COM_INTERFACE_ENTRY(IServiceProvider)
	COM_INTERFACE_ENTRY_AGGREGATE(IID_IMarshal, m_pUnkMarshaler.p)
	COM_INTERFACE_ENTRY(IVsInstalledProduct)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()
	DECLARE_GET_CONTROLLING_UNKNOWN()

	HRESULT FinalConstruct()
	{
		return CoCreateFreeThreadedMarshaler(
			GetControllingUnknown(), &m_pUnkMarshaler.p);
	}

	void FinalRelease()
	{
		m_pUnkMarshaler.Release();
	}

	CComPtr<IUnknown> m_pUnkMarshaler;

public:
	/********** IVsPackage **********/

    STDMETHODIMP SetSite( 
        /* [in] */ __RPC__in_opt IServiceProvider *pSP);

    STDMETHODIMP QueryClose( 
        /* [out] */ __RPC__out BOOL *pfCanClose)
	{
		*pfCanClose = TRUE;
		return S_OK;
	}
    
    STDMETHODIMP Close();
    
    STDMETHODIMP GetAutomationObject( 
        /* [in] */ __RPC__in LPCOLESTR pszPropName,
        /* [out] */ __RPC__deref_out_opt IDispatch **ppDisp)
	{
		ATLTRACENOTIMPL(_T("Package::GetAutomationObject"));
	}
    
    STDMETHODIMP CreateTool( 
        /* [in] */ __RPC__in REFGUID rguidPersistenceSlot)
	{
		ATLTRACENOTIMPL(_T("Package::CreateTool"));
	}
    
    STDMETHODIMP ResetDefaults( 
        /* [in] */ VSPKGRESETFLAGS grfFlags)
	{
		ATLTRACENOTIMPL(_T("Package::ResetDefaults"));
	}
    
    STDMETHODIMP GetPropertyPage( 
        /* [in] */ __RPC__in REFGUID rguidPage,
        /* [out][in] */ __RPC__inout VSPROPSHEETPAGE *ppage)
	{
		ATLTRACENOTIMPL(_T("Package::GetPropertyPage"));
	}


	/********** IServiceProvider **********/

    STDMETHODIMP QueryService( 
        /* [in] */ REFGUID guidService,
        /* [in] */ REFIID riid,
        /* [out] */ void __RPC_FAR *__RPC_FAR *ppvObject);


	/********** IVsInstalledProduct **********/

    STDMETHODIMP get_IdBmpSplash( 
        /* [retval][out] */ __RPC__out UINT *pIdBmp)
	{		
		ATLTRACENOTIMPL(_T("Package::get_IdBmpSplash"));
	}
    
    STDMETHODIMP get_OfficialName( 
        /* [retval][out] */ __RPC__deref_out_opt BSTR *pbstrName);
    
    STDMETHODIMP get_ProductID( 
        /* [retval][out] */ __RPC__deref_out_opt BSTR *pbstrPID);

    STDMETHODIMP get_ProductDetails( 
        /* [retval][out] */ __RPC__deref_out_opt BSTR *pbstrProductDetails);
    
    STDMETHODIMP get_IdIcoLogoForAboutbox( 
        /* [retval][out] */ __RPC__out UINT *pIdIco)
	{
		*pIdIco = IDI_ORBZ_LIGHTNING;
		return S_OK;
	}
};

OBJECT_ENTRY_AUTO(__uuidof(Package), Package)


#include "stdafx.h"
#include "Package.h"
#include "Language.h"
#include "dllmain.h"

#include "..\..\CommonVersionInfo.h"

STDMETHODIMP Package::get_OfficialName( 
    /* [retval][out] */ __RPC__deref_out_opt BSTR *pbstrName) 
{
	*pbstrName = CComBSTR(VERSIONINFO_PRODUCT).Detach();
	return S_OK;
}

STDMETHODIMP Package::get_ProductID( 
    /* [retval][out] */ __RPC__deref_out_opt BSTR *pbstrPID)
{
	*pbstrPID = CComBSTR("Build " VERSIONINFO_VERSIONSTRING).Detach();
	return S_OK;
}

STDMETHODIMP Package::get_ProductDetails( 
    /* [retval][out] */ __RPC__deref_out_opt BSTR *pbstrProductDetails)
{
	*pbstrProductDetails = CComBSTR(VERSIONINFO_PRODUCT " integration package for Visual Studio 2008. For more information see http://sparkviewengine.com.\r\n" VERSIONINFO_COPYRIGHT).Detach();
	return S_OK;
}

CComBSTR GetModulePath()
{
	WCHAR wszModule[MAX_PATH + 2];
	DWORD dwModuleLength = GetModuleFileNameW(_AtlBaseModule.GetModuleInstance(), wszModule, MAX_PATH);

	while (dwModuleLength != 0 && wszModule[dwModuleLength - 1] != L'\\')
		wszModule[--dwModuleLength] = '\0';

	return wszModule;
}

STDMETHODIMP Package::SetSite(IServiceProvider* site)
{
	_site = site;

	HRESULT hr = S_OK;

	// Create language object
	LanguageInit init = {_site};
	_HR(Language::CreateInstance(init, &_language));

	// Inform our site we offer the language
	CComPtr<IProfferService> proffer;
	_HR(_site->QueryService(SID_SProfferService, &proffer));
	_HR(proffer->ProfferService(__uuidof(SparkLanguageService), this, &_dwProfferCookie));

	// Create an appdomain based out of the location of this com dll. 
	// Managed Spark.dll and SparkLanguage.dll assemblies are in the same location
	CComPtr<ICorRuntimeHost> pRuntime;		
	_HR(CorBindToRuntimeEx(NULL, NULL, 0, CLSID_CorRuntimeHost, __uuidof(pRuntime), (void**)&pRuntime));

	CComPtr<IUnknown> punkSetup;
	_HR(pRuntime->CreateDomainSetup(&punkSetup));

	CComPtr<mscorlib::IAppDomainSetup> domainSetup;
	_HR(punkSetup->QueryInterface(&domainSetup));
	_HR(domainSetup->put_ApplicationBase(GetModulePath()));
	_HR(domainSetup->put_ApplicationName(CComBSTR(L"Spark Language Package Domain")));

	CComPtr<IUnknown> punkDomain;
	_HR(pRuntime->CreateDomainEx(L"Spark Language", punkSetup, NULL, &punkDomain));

	CComPtr<mscorlib::_AppDomain> appDomain;
	_HR(punkDomain->QueryInterface(&appDomain));


	// Create a LanguageSupervisor (and CCW) that is the root of all access into managed code
	CComPtr<mscorlib::_ObjectHandle> supervisorHandle = NULL;
	_HR(appDomain->CreateInstance(CComBSTR(L"SparkLanguage"), CComBSTR(L"SparkLanguage.LanguageSupervisor"), &supervisorHandle));

	CComVariant varSupervisor;
	_HR(supervisorHandle->Unwrap(&varSupervisor));
	_HR(varSupervisor.ChangeType(VT_UNKNOWN));

	CComPtr<ILanguageSupervisor> supervisor;
	_HR(V_UNKNOWN(&varSupervisor)->QueryInterface(&supervisor));

	// Associate the objects	
	_HR(_language->SetSupervisor(supervisor));

	return hr;
}

STDMETHODIMP Package::QueryService(REFGUID guidService, REFIID riid, void** ppvObject)
{
	if (guidService == __uuidof(SparkLanguageService))
	{
		//SparkLanguage::Language^ lang = gcnew SparkLanguage::Language();
		//CComPtr<IUnknown> punk = (IUnknown*)System::Runtime::InteropServices::Marshal::GetIUnknownForObject(lang).ToPointer();
		//return punk->QueryInterface(riid, ppvObject);
		return _language->QueryInterface(riid, ppvObject);
	}

	return _site->QueryService(guidService, riid, ppvObject);
}

STDMETHODIMP Package::Close()
{
	HRESULT hr = S_OK;
	if (_site != NULL && _dwProfferCookie != 0)
	{
		CComPtr<IProfferService> proffer;
		_HR(_site->QueryService(SID_SProfferService, &proffer));
		_HR(proffer->RevokeService(_dwProfferCookie));
		_dwProfferCookie = 0;
	}

	_site = NULL;
	return hr;
}


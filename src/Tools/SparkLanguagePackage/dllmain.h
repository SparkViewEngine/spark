// dllmain.h : Declaration of module class.

class CSparkLanguagePackageModule : public CAtlDllModuleT< CSparkLanguagePackageModule >
{
public :
//	DECLARE_LIBID(LIBID_SparkLanguagePackageLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_SPARKLANGUAGEPACKAGE, "{067FE4A3-E31E-4043-AE0E-ECF1439A6D28}")

	virtual HRESULT AddCommonRGSReplacements(IRegistrarBase* pRegistrar) throw()
	{
		HRESULT hr = CAtlDllModuleT< CSparkLanguagePackageModule >::AddCommonRGSReplacements(pRegistrar);
		
		WCHAR wszModule[MAX_PATH + 2];
		DWORD dwModuleLength = GetModuleFileNameW(_AtlBaseModule.GetModuleInstance(), wszModule, MAX_PATH);
		while (dwModuleLength != 0 && wszModule[dwModuleLength - 1] != L'\\')
			wszModule[--dwModuleLength] = '\0';

		if (SUCCEEDED(hr))
			hr = pRegistrar->AddReplacement(L"Module_Path", wszModule);

		return hr;
	}
};

extern class CSparkLanguagePackageModule _AtlModule;

// dllmain.h : Declaration of module class.

class CSparkLanguagePackageModule : public CAtlDllModuleT< CSparkLanguagePackageModule >
{
public :
//	DECLARE_LIBID(LIBID_SparkLanguagePackageLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_SPARKLANGUAGEPACKAGE, "{067FE4A3-E31E-4043-AE0E-ECF1439A6D28}")
};

extern class CSparkLanguagePackageModule _AtlModule;

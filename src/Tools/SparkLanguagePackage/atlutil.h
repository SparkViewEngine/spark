
#pragma once


#ifdef _DEBUG
#define _HR(statement) if (SUCCEEDED(hr)) {hr = _HRLOG(statement, #statement, __FILE__, __LINE__);}
__declspec(selectany) CTraceCategory FAILED_HRESULT(_T("Failed HRESULT"), 1);

inline HRESULT _HRLOG(HRESULT hr, LPCSTR statement, LPCSTR file, int line)
{
	if (FAILED(hr))
	{
		ATL::CTraceFileAndLineInfo(file, line)(FAILED_HRESULT, 1, "0x%08x %s\n", hr, statement);
	}
	return hr;
}
#else
#define _HR(statement) if (SUCCEEDED(hr)) {hr = (statement);}
#endif

template<class T, class TInit>
class CComCreatableObject : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public TInit
{
public:
	static HRESULT CreateInstance(TInit& init, REFIID riid, void** ppv)
	{
		return CComCreator<CComObject<T>>::CreateInstance(&init, riid, ppv);
	}

	template<class Q>
	static HRESULT CreateInstance(TInit& init, Q** ppItf)
	{
		return CComCreator<CComObject<T>>::CreateInstance(&init, __uuidof(Q), (void**)ppItf);
	}

	void SetVoid(void* pv)
	{
		*(TInit*)this = *(TInit*)pv;
	}
};


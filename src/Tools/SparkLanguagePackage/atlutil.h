
#pragma once

#define _HR(statement) if (SUCCEEDED(hr)) {hr = (statement);}

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


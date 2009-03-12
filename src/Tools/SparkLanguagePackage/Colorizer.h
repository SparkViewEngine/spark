
#pragma once

#include "atlutil.h"
#include "SparkLanguagePackage_i.h"

class ColorizerInit
{
public:
	CComPtr<ISparkLanguage> _language;
	CComPtr<IVsTextLines> _buffer;
	int _containedLanguageColorCount;
};

class ATL_NO_VTABLE Colorizer:
	public CComCreatableObject<Colorizer, ColorizerInit>,
	public IVsColorizer,
	public IVsColorizer2
{	
	CComPtr<ISparkSource> _source;
	CComPtr<IVsContainedLanguageColorizer> _containedColorizer;

	long _paintLength;
	SourcePainting* _paintArray;

public:
	Colorizer()
	{
		_paintLength = 0;
		_paintArray = NULL;
	}

	BEGIN_COM_MAP(Colorizer)
		COM_INTERFACE_ENTRY(IVsColorizer)
		COM_INTERFACE_ENTRY(IVsColorizer2)
	END_COM_MAP()

	HRESULT FinalConstruct();


	/**** IVsColorizer2 ****/

	STDMETHODIMP GetStateMaintenanceFlag( 
		/* [out] */ __RPC__out BOOL *pfFlag)
	{
		*pfFlag = FALSE;
		return S_OK;
	}
        
    STDMETHODIMP GetStartState( 
        /* [out] */ __RPC__out long *piStartState)
	{
		*piStartState = 0;
		return S_OK;
	}
    
    STDMETHODIMP_(long) ColorizeLine( 
        /* [in] */ long iLine,
        /* [in] */ long iLength,
        /* [in] */ __RPC__in const WCHAR *pszText,
        /* [in] */ long iState,
        /* [out] */ __RPC__out ULONG *pAttributes);
    
    STDMETHODIMP_(long) GetStateAtEndOfLine( 
        /* [in] */ long iLine,
        /* [in] */ long iLength,
        /* [in] */ __RPC__in const WCHAR *pText,
        /* [in] */ long iState)
	{
		return 0;
	}
    
    STDMETHODIMP_(void) CloseColorizer( void)
	{		
	}


	/**** IVsColorizer2 ****/
	
	STDMETHODIMP BeginColorization();
    
	STDMETHODIMP EndColorization() 
	{
		return S_OK;
	}
};


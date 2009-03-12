
#pragma once

#include "atlutil.h"
#include "SparkLanguagePackage_i.h"

class ColorableItemInit
{
public:
	LPCWSTR _name;
	COLORINDEX _foreground;
	COLORINDEX _background;
	DWORD _fontFlags;
};

class ColorableItem :
	public CComCreatableObject<ColorableItem, ColorableItemInit>,
	public IVsColorableItem
{
public:
	BEGIN_COM_MAP(ColorableItem)
		COM_INTERFACE_ENTRY(IVsColorableItem)
	END_COM_MAP()

	/**** IVsColorableItem ****/
    STDMETHODIMP GetDefaultColors( 
        /* [out] */ __RPC__out COLORINDEX *piForeground,
        /* [out] */ __RPC__out COLORINDEX *piBackground)
	{
		*piForeground = _foreground;
		*piBackground = _background;
		return S_OK;
	}
    
    STDMETHODIMP GetDefaultFontFlags( 
        /* [out] */ __RPC__out DWORD *pdwFontFlags)
	{
		*pdwFontFlags = _fontFlags;
		return S_OK;
	}
    
    STDMETHODIMP GetDisplayName( 
		/* [out] */ __RPC__deref_out_opt BSTR *pbstrName)
	{
		*pbstrName = SysAllocString(_name);
		return S_OK;
	}
};


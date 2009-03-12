
#pragma once

#include "atlutil.h"
#include "SparkLanguagePackage_i.h"

class TextViewFilterInit
{
public:
	CComPtr<ISparkLanguage> _language;
	CComPtr<IVsTextView> _textView;
};

class TextViewFilter : 
	public CComCreatableObject<TextViewFilter, TextViewFilterInit>,
	public IVsTextViewFilter,
	public IOleCommandTarget
{
	CComPtr<ISparkSource> _source;
	
	CComPtr<IVsTextViewIntellisenseHost> _intellisenseHost;

	CComPtr<IOleCommandTarget> _nextCommandTarget;

	CComPtr<IOleCommandTarget> _containedCommandTarget;
	CComPtr<IVsTextViewFilter> _containedTextViewFilter;

	CComQIPtr<IOleCommandTarget> _chainCommandTarget;
	CComQIPtr<IVsTextViewFilter> _chainTextViewFilter;

public:
	BEGIN_COM_MAP(TextViewFilter)
		COM_INTERFACE_ENTRY(IVsTextViewFilter)
		COM_INTERFACE_ENTRY(IOleCommandTarget)
	END_COM_MAP()

	
	DECLARE_PROTECT_FINAL_CONSTRUCT();

	HRESULT FinalConstruct();


	/* IVsTextViewFilter */
    STDMETHODIMP GetWordExtent( 
        /* [in] */ long iLine,
        /* [in] */ CharIndex iIndex,
        /* [in] */ DWORD dwFlags,
        /* [out] */ __RPC__out TextSpan *pSpan)
	{
		if (_chainTextViewFilter == NULL)
			return S_OK;

		return _chainTextViewFilter->GetWordExtent(iLine, iIndex, dwFlags, pSpan);
	}
    
    STDMETHODIMP GetDataTipText( 
        /* [out][in] */ __RPC__inout TextSpan *pSpan,
        /* [out] */ __RPC__deref_out_opt BSTR *pbstrText)
	{
		if (_chainTextViewFilter == NULL)
			return S_OK;

		return _chainTextViewFilter->GetDataTipText(pSpan, pbstrText);
	}
    
    STDMETHODIMP GetPairExtents( 
        /* [in] */ long iLine,
        /* [in] */ CharIndex iIndex,
        /* [out] */ __RPC__out TextSpan *pSpan)
	{
		if (_chainTextViewFilter == NULL)
			return S_OK;

		return _chainTextViewFilter->GetPairExtents(iLine, iIndex, pSpan);
	}

	/* IOleCommandTarget */
    STDMETHODIMP QueryStatus( 
        /* [unique][in] */ __RPC__in_opt const GUID *pguidCmdGroup,
        /* [in] */ ULONG cCmds,
        /* [out][in][size_is] */ __RPC__inout_ecount_full(cCmds) OLECMD prgCmds[  ],
        /* [unique][out][in] */ __RPC__inout_opt OLECMDTEXT *pCmdText);
    
    STDMETHODIMP Exec( 
        /* [unique][in] */ __RPC__in_opt const GUID *pguidCmdGroup,
        /* [in] */ DWORD nCmdID,
        /* [in] */ DWORD nCmdexecopt,
        /* [unique][in] */ __RPC__in_opt VARIANT *pvaIn,
        /* [unique][out][in] */ __RPC__inout_opt VARIANT *pvaOut);
};

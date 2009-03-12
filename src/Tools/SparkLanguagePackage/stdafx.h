// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#ifndef STRICT
#define STRICT
#endif

#include "targetver.h"

#define _ATL_APARTMENT_THREADED
#define _ATL_NO_AUTOMATIC_NAMESPACE

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// some CString constructors will be explicit


//#define _ATL_DEBUG_INTERFACES
//#define _ATL_DEBUG_QI

#include "resource.h"
#include <atlbase.h>
#include <atlcom.h>
#include <atlctl.h>

using namespace ATL;


// include visual studio sdk

#define Project DTE_Project
#define Language DTE_Language


#include <vsshell.h>
#include <vsshell90.h>
#include <vslangproj.h>
#include <singlefileeditor.h>
#include <textmgr.h>
#include <textmgr2.h>
#include <containedlanguage.h>
#include <webapplicationctx.h>
#include <vssplash.h>

#undef Project
#undef Language

class __declspec(uuid("694DD9B6-B865-4C5B-AD85-86356E9C88DC")) CSharp;
class __declspec(uuid("7D842D0C-FDD6-4e3b-9E21-0C263F4B6EC2")) CSharpIntellisenseProvider;

// include clr unmanaged access

#include <mscoree.h>
#import  <mscorlib.tlb> raw_interfaces_only high_property_prefixes("_get","_put","_putref")
#pragma comment(lib,"mscoree.lib")


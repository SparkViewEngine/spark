using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SparkLanguagePackageLib;

namespace SparkLanguage.VsAdapters
{
    public class HierarchyItem : IEnumerable<HierarchyItem>
    {
        readonly IVsHierarchy _hierarchy;
        readonly uint _itemId;

        public HierarchyItem(IVsHierarchy hierarchy, uint itemId)
        {
            _hierarchy = hierarchy;
            _itemId = itemId;
        }

        public string Name
        {
            get
            {
                object val;
                _hierarchy.GetProperty(_itemId, (int)__VSHPROPID.VSHPROPID_Name, out val);
                return (string)val;
            }
        }

        HierarchyItem RelativeItem(__VSHPROPID propid)
        {
            object val;
            _hierarchy.GetProperty(_itemId, (int)propid, out val);
            uint itemId = (uint)(int)val;
            if (itemId == uint.MaxValue)
                return null;
            return new HierarchyItem(_hierarchy, itemId);
        }

        public HierarchyItem FirstChild
        {
            get { return RelativeItem(__VSHPROPID.VSHPROPID_FirstChild); }
        }

        public HierarchyItem NextSibling
        {
            get { return RelativeItem(__VSHPROPID.VSHPROPID_NextSibling); }
        }

        public HierarchyItem Parent
        {
            get { return RelativeItem(__VSHPROPID.VSHPROPID_Parent); }
        }

        public HierarchyItem FindPath(string path)
        {
            int slashPos = path.IndexOfAny(new[] { '/', '\\' });
            var segment = slashPos == -1 ? path : path.Substring(0, slashPos);
            for (var child = FirstChild; child != null; child = child.NextSibling)
            {
                if (string.Equals(segment, child.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return slashPos == -1 ? child : child.FindPath(path.Substring(slashPos + 1));
                }
            }
            return null;
        }

        public string CanonicalName
        {
            get
            {
                string name;
                _hierarchy.GetCanonicalName(_itemId, out name);
                return name;
            }
        }


        public override int GetHashCode()
        {
            var hash = _itemId.GetHashCode();
            if (_hierarchy != null) 
                hash ^= _hierarchy.GetHashCode();
            return hash;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;
            if (!Equals(_hierarchy, ((HierarchyItem)obj)._hierarchy))
                return false;
            return _itemId == ((HierarchyItem)obj)._itemId;
        }

        public IEnumerator<HierarchyItem> GetEnumerator()
        {
            var scan = FirstChild;
            while (scan != null)
            {
                yield return scan;
                scan = scan.NextSibling;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public enum __VSHPROPID
    {
        VSHPROPID_DefaultEnableDeployProjectCfg = -2064,
        VSHPROPID_FIRST = -2064,
        VSHPROPID_DefaultEnableBuildProjectCfg = -2063,
        VSHPROPID_HasEnumerationSideEffects = -2062,
        VSHPROPID_DesignerFunctionVisibility = -2061,
        VSHPROPID_DesignerVariableNaming = -2060,
        VSHPROPID_ProjectIDGuid = -2059,
        VSHPROPID_ShowOnlyItemCaption = -2058,
        VSHPROPID_IsNewUnsavedItem = -2057,
        VSHPROPID_AllowEditInRunMode = -2056,
        VSHPROPID_ShowProjInSolutionPage = -2055,
        VSHPROPID_PreferredLanguageSID = -2054,
        VSHPROPID_CanBuildFromMemory = -2053,
        VSHPROPID_IsFindInFilesForegroundOnly = -2052,
        VSHPROPID_IsNonSearchable = -2051,
        VSHPROPID_DefaultNamespace = -2049,
        VSHPROPID_OverlayIconIndex = -2048,
        VSHPROPID_ItemSubType = -2047,
        VSHPROPID_StorageType = -2046,
        VSHPROPID_IsNonLocalStorage = -2045,
        VSHPROPID_IsNonMemberItem = -2044,
        VSHPROPID_IsHiddenItem = -2043,
        VSHPROPID_NextVisibleSibling = -2042,
        VSHPROPID_FirstVisibleChild = -2041,
        VSHPROPID_StartupServices = -2040,
        VSHPROPID_OwnerKey = -2038,
        VSHPROPID_ImplantHierarchy = -2037,
        VSHPROPID_ConfigurationProvider = -2036,
        VSHPROPID_Expanded = -2035,
        VSHPROPID_ItemDocCookie = -2034,
        VSHPROPID_ParentHierarchyItemid = -2033,
        VSHPROPID_ParentHierarchy = -2032,
        VSHPROPID_HandlesOwnReload = -2031,
        VSHPROPID_ReloadableProjectFile = -2031,
        VSHPROPID_ProjectType = -2030,
        VSHPROPID_TypeName = -2030,
        VSHPROPID_StateIconIndex = -2029,
        VSHPROPID_ExtSelectedItem = -2028,
        VSHPROPID_ExtObject = -2027,
        VSHPROPID_EditLabel = -2026,
        VSHPROPID_UserContext = -2023,
        VSHPROPID_SortPriority = -2022,
        VSHPROPID_ProjectDir = -2021,
        VSHPROPID_AltItemid = -2020,
        VSHPROPID_AltHierarchy = -2019,
        VSHPROPID_BrowseObject = -2018,
        VSHPROPID_SelContainer = -2017,
        VSHPROPID_CmdUIGuid = -2016,
        VSHPROPID_OpenFolderIconIndex = -2015,
        VSHPROPID_OpenFolderIconHandle = -2014,
        VSHPROPID_IconHandle = -2013,
        VSHPROPID_Name = -2012,
        VSHPROPID_ProjectName = -2012,
        VSHPROPID_ExpandByDefault = -2011,
        VSHPROPID_Expandable = -2006,
        VSHPROPID_IconIndex = -2005,
        VSHPROPID_IconImgList = -2004,
        VSHPROPID_Caption = -2003,
        VSHPROPID_SaveName = -2002,
        VSHPROPID_TypeGuid = -1004,
        VSHPROPID_Root = -1003,
        VSHPROPID_NextSibling = -1002,
        VSHPROPID_FirstChild = -1001,
        VSHPROPID_Parent = -1000,
        VSHPROPID_LAST = -1000,
        VSHPROPID_NIL = -1,
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider=Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace SparkLanguage.Tests
{
    public class StubPackageSite : IServiceProvider, IProfferService
    {
        public TInterface QueryService<TInterface, TService>()
        {
            return QueryService<TInterface>(typeof (TService));
        }

        public TInterface QueryService<TInterface>(Type service)
        {
            var guidService = service.GUID;
            var iid = typeof(TInterface).GUID;
            IntPtr ppvObject;
            var hr = QueryService(ref guidService, ref iid, out ppvObject);
            if (hr != VSConstants.S_OK)
                throw new COMException("QueryService failed", hr);

            var retval = Marshal.GetObjectForIUnknown(ppvObject);
            Marshal.Release(ppvObject);
            return (TInterface)retval;
        }

        public int QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject)
        {
            var sidProfferService = new Guid("{cb728b20-f786-11ce-92ad-00aa00a74cd0}");
            var csharp = new Guid("{694dd9b6-b865-4c5b-ad85-86356e9c88dc}");

            if (guidService == sidProfferService)
            {
                return ServiceFrom(this, ref riid, out ppvObject);
            }
            if (guidService == csharp)
            {
                return ServiceFrom(new StubContainedLanguageService(), ref riid, out ppvObject);
            }
            if (_providers.ContainsKey(guidService))
            {
                return _providers[guidService].QueryService(ref guidService, ref riid, out ppvObject);
            }
            throw new NotImplementedException();
        }

        private static int ServiceFrom(object instance, ref Guid riid, out IntPtr ppvObject)
        {
            var punkThis = Marshal.GetIUnknownForObject(instance);
            var retval = Marshal.QueryInterface(punkThis, ref riid, out ppvObject);
            Marshal.Release(punkThis);
            return retval;
        }

        Dictionary<Guid, IServiceProvider> _providers = new Dictionary<Guid, IServiceProvider>();
        public int ProfferService(ref Guid rguidService, IServiceProvider psp, out uint pdwCookie)
        {
            _providers.Add(rguidService, psp);
            pdwCookie = 1;
            return VSConstants.S_OK;
        }

        public int RevokeService(uint dwCookie)
        {
            return VSConstants.S_OK;
        }
    }
}
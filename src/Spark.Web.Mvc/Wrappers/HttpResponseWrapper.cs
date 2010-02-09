// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace Spark.Web.Mvc.Wrappers
{
    public class HttpResponseWrapper : HttpResponseBase
    {
        // Fields
        private readonly HttpResponseBase _httpResponse;
        private readonly ITextWriterContainer _textWriterContainer;

        // Methods
        public HttpResponseWrapper(HttpResponseBase httpResponse, ITextWriterContainer textWriterContainer)
        {
            if (httpResponse == null)
            {
                throw new ArgumentNullException("httpResponse");
            }
            _httpResponse = httpResponse;
            _textWriterContainer = textWriterContainer;
        }

        public override bool Buffer
        {
            get { return _httpResponse.Buffer; }
            set { _httpResponse.Buffer = value; }
        }

        public override bool BufferOutput
        {
            get { return _httpResponse.BufferOutput; }
            set { _httpResponse.BufferOutput = value; }
        }

        public override HttpCachePolicyBase Cache
        {
            get { return _httpResponse.Cache; }
        }

        public override string CacheControl
        {
            get { return _httpResponse.CacheControl; }
            set { _httpResponse.CacheControl = value; }
        }

        public override string Charset
        {
            get { return _httpResponse.Charset; }
            set { _httpResponse.Charset = value; }
        }

        public override Encoding ContentEncoding
        {
            get { return _httpResponse.ContentEncoding; }
            set { _httpResponse.ContentEncoding = value; }
        }

        public override string ContentType
        {
            get { return _httpResponse.ContentType; }
            set { _httpResponse.ContentType = value; }
        }

        public override HttpCookieCollection Cookies
        {
            get { return _httpResponse.Cookies; }
        }

        public override int Expires
        {
            get { return _httpResponse.Expires; }
            set { _httpResponse.Expires = value; }
        }

        public override DateTime ExpiresAbsolute
        {
            get { return _httpResponse.ExpiresAbsolute; }
            set { _httpResponse.ExpiresAbsolute = value; }
        }

        public override Stream Filter
        {
            get { return _httpResponse.Filter; }
            set { _httpResponse.Filter = value; }
        }

        public override Encoding HeaderEncoding
        {
            get { return _httpResponse.HeaderEncoding; }
            set { _httpResponse.HeaderEncoding = value; }
        }

        public override NameValueCollection Headers
        {
            get { return _httpResponse.Headers; }
        }

        public override bool IsClientConnected
        {
            get { return _httpResponse.IsClientConnected; }
        }

        public override bool IsRequestBeingRedirected
        {
            get { return _httpResponse.IsRequestBeingRedirected; }
        }

        public override TextWriter Output
        {
            get { return _textWriterContainer.Output; }
        }

        public override Stream OutputStream
        {
            get { throw new HttpException("OutputStream not available"); }
        }

        public override string RedirectLocation
        {
            get { return _httpResponse.RedirectLocation; }
            set { _httpResponse.RedirectLocation = value; }
        }

        public override string Status
        {
            get { return _httpResponse.Status; }
            set { _httpResponse.Status = value; }
        }

        public override int StatusCode
        {
            get { return _httpResponse.StatusCode; }
            set { _httpResponse.StatusCode = value; }
        }

        public override string StatusDescription
        {
            get { return _httpResponse.StatusDescription; }
            set { _httpResponse.StatusDescription = value; }
        }

        public override int SubStatusCode
        {
            get { return _httpResponse.SubStatusCode; }
            set { _httpResponse.SubStatusCode = value; }
        }

        public override bool SuppressContent
        {
            get { return _httpResponse.SuppressContent; }
            set { _httpResponse.SuppressContent = value; }
        }

        public override bool TrySkipIisCustomErrors
        {
            get { return _httpResponse.TrySkipIisCustomErrors; }
            set { _httpResponse.TrySkipIisCustomErrors = value; }
        }

        public override void AddCacheDependency(params CacheDependency[] dependencies)
        {
            _httpResponse.AddCacheDependency(dependencies);
        }

        public override void AddCacheItemDependencies(ArrayList cacheKeys)
        {
            _httpResponse.AddCacheItemDependencies(cacheKeys);
        }

        public override void AddCacheItemDependencies(string[] cacheKeys)
        {
            _httpResponse.AddCacheItemDependencies(cacheKeys);
        }

        public override void AddCacheItemDependency(string cacheKey)
        {
            _httpResponse.AddCacheItemDependency(cacheKey);
        }

        public override void AddFileDependencies(string[] filenames)
        {
            _httpResponse.AddFileDependencies(filenames);
        }

        public override void AddFileDependencies(ArrayList filenames)
        {
            _httpResponse.AddFileDependencies(filenames);
        }

        public override void AddFileDependency(string filename)
        {
            _httpResponse.AddFileDependency(filename);
        }

        public override void AddHeader(string name, string value)
        {
            _httpResponse.AddHeader(name, value);
        }

        public override void AppendCookie(HttpCookie cookie)
        {
            _httpResponse.AppendCookie(cookie);
        }

        public override void AppendHeader(string name, string value)
        {
            _httpResponse.AppendHeader(name, value);
        }

        public override void AppendToLog(string param)
        {
            _httpResponse.AppendToLog(param);
        }

        public override string ApplyAppPathModifier(string virtualPath)
        {
            return _httpResponse.ApplyAppPathModifier(virtualPath);
        }

        public override void BinaryWrite(byte[] buffer)
        {
            _httpResponse.BinaryWrite(buffer);
        }

        public override void Clear()
        {
            _httpResponse.Clear();
        }

        public override void ClearContent()
        {
            _httpResponse.ClearContent();
        }

        public override void ClearHeaders()
        {
            _httpResponse.ClearHeaders();
        }

        public override void Close()
        {
            _httpResponse.Close();
        }

        public override void DisableKernelCache()
        {
            _httpResponse.DisableKernelCache();
        }

        public override void End()
        {
            _httpResponse.End();
        }

        public override void Flush()
        {
            _httpResponse.Flush();
        }

        public override void Pics(string value)
        {
            _httpResponse.Pics(value);
        }

        public override void Redirect(string url)
        {
            _httpResponse.Redirect(url);
        }

        public override void Redirect(string url, bool endResponse)
        {
            _httpResponse.Redirect(url, endResponse);
        }

        public override void RemoveOutputCacheItem(string path)
        {
            _httpResponse.RemoveOutputCacheItem(path);
        }

        public override void SetCookie(HttpCookie cookie)
        {
            _httpResponse.SetCookie(cookie);
        }

        public override void TransmitFile(string filename)
        {
            _httpResponse.TransmitFile(filename);
        }

        public override void TransmitFile(string filename, long offset, long length)
        {
            _httpResponse.TransmitFile(filename, offset, length);
        }

        public override void Write(char ch)
        {
            Output.Write(ch);
        }

        public override void Write(object obj)
        {
            Output.Write(obj);
        }

        public override void Write(string s)
        {
            Output.Write(s);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Output.Write(buffer, index, count);
        }

        public override void WriteFile(string filename)
        {
            _httpResponse.WriteFile(filename);
        }

        public override void WriteFile(string filename, bool readIntoMemory)
        {
            _httpResponse.WriteFile(filename, readIntoMemory);
        }

        public override void WriteFile(IntPtr fileHandle, long offset, long size)
        {
            _httpResponse.WriteFile(fileHandle, offset, size);
        }

        public override void WriteFile(string filename, long offset, long size)
        {
            _httpResponse.WriteFile(filename, offset, size);
        }

        public override void WriteSubstitution(HttpResponseSubstitutionCallback callback)
        {
            _httpResponse.WriteSubstitution(callback);
        }

        // Properties
    }
}
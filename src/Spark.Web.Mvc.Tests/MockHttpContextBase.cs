﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Rhino.Mocks;

namespace Spark.Web.Mvc.Tests
{
    public class MockHttpContextBase
    {
        public static HttpContextBase Generate(string path)
        {
            return Generate(path, new StringWriter(), new MemoryStream());
        }

        public static HttpContextBase Generate(string path, TextWriter output)
        {
            return Generate(path, output, new MemoryStream());
        }

        public static HttpContextBase Generate(string path, Stream outputStream)
        {
            return Generate(path, new StringWriter(), outputStream);
        }

        public static HttpContextBase Generate(string path, TextWriter output, Stream outputStream)
        {
            var contextBase = MockRepository.GenerateStub<HttpContextBase>();
            var requestBase = MockRepository.GenerateStub<HttpRequestBase>();
            var responseBase = MockRepository.GenerateStub<HttpResponseBase>();
            var sessionStateBase = MockRepository.GenerateStub<HttpSessionStateBase>();
            var serverUtilityBase = MockRepository.GenerateStub<HttpServerUtilityBase>();
            var items = new Dictionary<object, object>();

            contextBase.Stub(x => x.Request).Return(requestBase);
            contextBase.Stub(x => x.Response).Return(responseBase);
            contextBase.Stub(x => x.Session).Return(sessionStateBase);
            contextBase.Stub(x => x.Server).Return(serverUtilityBase);
            contextBase.Stub(x => x.Items).Return(items);

            responseBase.Output = output;
            responseBase.Stub(x => x.OutputStream).Return(outputStream);


            requestBase.Stub(x => x.ApplicationPath).Return("/");
            requestBase.Stub(x => x.Path).Return(path);
            responseBase.Stub(x => x.ApplyAppPathModifier(null))
                .IgnoreArguments()
                .Do(new Func<string, string>(x => x));

            return contextBase;
        }
    }
}

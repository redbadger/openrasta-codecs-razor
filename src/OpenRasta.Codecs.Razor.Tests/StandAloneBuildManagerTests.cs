﻿//-------------------------------------------------------------------------------------------------
// <auto-generated> 
// Marked as auto-generated so StyleCop will ignore BDD style tests
// </auto-generated>
//-------------------------------------------------------------------------------------------------


#pragma warning disable 169
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace OpenRasta.Codecs.Razor.Tests
{
    using System;
    using System.Web;

    using NUnit.Framework;

    [TestFixture]
    public class CompilationManagerTests
    {
        [Test]
        public void Invalid_code_causes_exception_to_be_thrown()
        {
            IBuildManager manager =
                new StandAloneBuildManager(new FakeViewProvider(@"@resource System.String
@System.DateTime.Now2"));
            Assert.Throws<HttpCompileException>(() => manager.GetCompiledType("ViewFile.cshtml"));
        }

        [Test]
        public void Valid_code_is_compiled_to_a_class()
        {
            IBuildManager manager =
                new StandAloneBuildManager(new FakeViewProvider(@"@resource System.String
@System.DateTime.Now"));
            Type type = manager.GetCompiledType("ViewFile.cshtml");
            Assert.IsNotNull(type);
        }
    }
}
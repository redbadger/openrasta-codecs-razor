﻿//-------------------------------------------------------------------------------------------------
// <auto-generated> 
// Marked as auto-generated so StyleCop will ignore BDD style tests
// </auto-generated>
//-------------------------------------------------------------------------------------------------

#pragma warning disable 169
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace OpenRasta.Codecs.Razor.Specs
{
    using System;
    using System.Web;

    using Machine.Specifications;

    public class when_compiling_a_view_containing_bad_code
    {
        private static Exception exception;

        private static StandAloneBuildManager manager;

        private Establish context =
            () =>
            manager =
            new StandAloneBuildManager(new FakeViewProvider(@"@resource System.String
@System.DateTime.Now2"));

        private Because of = () => exception = Catch.Exception(() => manager.GetCompiledType("ViewFile.cshtml"));

        private It should_throw_an_HttpCompileException = () => exception.ShouldBeOfType<HttpCompileException>();
    }

    public class when_compiling_a_view_containing_good_code
    {
        private static StandAloneBuildManager manager;

        private static Type type;

        private Establish context =
            () =>
            manager = new StandAloneBuildManager(new FakeViewProvider(@"@resource System.String
@System.DateTime.Now"));

        private Because of = () => type = manager.GetCompiledType("ViewFile.cshtml");

        private It should_compile_the_view = () => type.ShouldNotBeNull();
    }
}
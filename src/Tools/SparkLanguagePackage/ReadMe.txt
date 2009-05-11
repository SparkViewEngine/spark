========================================================================
    SparkLanguagePackage Project Overview
========================================================================

=== set up ===

"C:\Program Files (x86)\Microsoft Visual Studio 2008 SDK\VisualStudioIntegration\Tools\Bin\VSRegEx.exe" GetOrig 9.0 Exp RANU


SparkLanguagePackage debug build:

regsvr32.exe
/n /i:user "$(TargetPath)"


=== run with debugging ===

C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\IDE\devenv.exe
/ranu /rootsuffix Exp "some sln"


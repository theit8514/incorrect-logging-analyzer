﻿using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = IncorrectLoggingAnalyzer.Test.Verifiers.CSharpCodeFixVerifier<
    IncorrectLoggingAnalyzer.IncorrectLoggingAnalyzerAnalyzer,
    IncorrectLoggingAnalyzer.IncorrectLoggingAnalyzerCodeFixProvider>;

namespace IncorrectLoggingAnalyzer.Test
{
    [TestClass]
    public class IncorrectLoggingAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task DoesNotReturnSpuriousDiagnostic()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_ReplaceGenericType()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            private readonly {|#0:ILogger<OtherClass>|} _logger;

            public MyClass(ILogger<OtherClass> logger) => _logger = logger;
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            private readonly ILogger<MyClass> _logger;

            public MyClass(ILogger<MyClass> logger) => _logger = logger;
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_ReplaceQualifiedType()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            private readonly {|#0:Microsoft.Extensions.Logging.ILogger<OtherClass>|} _logger;

            public MyClass(Microsoft.Extensions.Logging.ILogger<OtherClass> logger) => _logger = logger;
        }
        class OtherClass
        {
            private readonly Microsoft.Extensions.Logging.ILogger<OtherClass> _logger;

            public OtherClass(Microsoft.Extensions.Logging.ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            private readonly Microsoft.Extensions.Logging.ILogger<MyClass> _logger;

            public MyClass(Microsoft.Extensions.Logging.ILogger<MyClass> logger) => _logger = logger;
        }
        class OtherClass
        {
            private readonly Microsoft.Extensions.Logging.ILogger<OtherClass> _logger;

            public OtherClass(Microsoft.Extensions.Logging.ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_ReplaceMixedType()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            private readonly {|#0:ILogger<OtherClass>|} _logger;

            public MyClass(Microsoft.Extensions.Logging.ILogger<OtherClass> logger) => _logger = logger;
        }
        class OtherClass
        {
            private readonly Microsoft.Extensions.Logging.ILogger<OtherClass> _logger;

            public OtherClass(Microsoft.Extensions.Logging.ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            private readonly ILogger<MyClass> _logger;

            public MyClass(Microsoft.Extensions.Logging.ILogger<MyClass> logger) => _logger = logger;
        }
        class OtherClass
        {
            private readonly Microsoft.Extensions.Logging.ILogger<OtherClass> _logger;

            public OtherClass(Microsoft.Extensions.Logging.ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_ReplaceMixedType2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            private readonly {|#0:Microsoft.Extensions.Logging.ILogger<OtherClass>|} _logger;

            public MyClass(ILogger<OtherClass> logger) => _logger = logger;
        }
        class OtherClass
        {
            private readonly Microsoft.Extensions.Logging.ILogger<OtherClass> _logger;

            public OtherClass(Microsoft.Extensions.Logging.ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            private readonly Microsoft.Extensions.Logging.ILogger<MyClass> _logger;

            public MyClass(ILogger<MyClass> logger) => _logger = logger;
        }
        class OtherClass
        {
            private readonly Microsoft.Extensions.Logging.ILogger<OtherClass> _logger;

            public OtherClass(Microsoft.Extensions.Logging.ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_IgnoresNonGenericLogger()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            private readonly {|#0:ILogger<OtherClass>|} _logger;
            private readonly ILogger _regularLogger;

            public MyClass(ILogger<OtherClass> logger, ILogger regularLogger) {
                _logger = logger;
                _regularLogger = regularLogger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_IgnoresOtherGeneric()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            private readonly {|#0:ILogger<OtherClass>|} _logger;
            private readonly List<int> _otherField;

            public MyClass(ILogger<OtherClass> logger, List<int> otherField) {
                _logger = logger;
                _otherField = otherField;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_IgnoresMultipleGeneric()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace Microsoft.Extensions.Logging {
        interface ILogger<TA, TB> { }
    }
    namespace ConsoleApplication1
    {
        class MyClass
        {
            private readonly {|#0:ILogger<OtherClass>|} _logger;
            private readonly ILogger<string, string> _otherField;

            public MyClass(ILogger<OtherClass> logger, ILogger<string, string> otherField) {
                _logger = logger;
                _otherField = otherField;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        //Diagnostic triggered and checked for
        [TestMethod]
        public async Task RuleStatic_ReportGenericType()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        static class MyClass
        {
            private static readonly {|#0:ILogger<OtherClass>|} _logger;
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var expected = VerifyCS.Diagnostic("ILA1002").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        //Diagnostic triggered and checked for
        [TestMethod]
        public async Task RuleStatic_ReportQualifiedType()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        static class MyClass
        {
            private static readonly {|#0:Microsoft.Extensions.Logging.ILogger<OtherClass>|} _logger;
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            var expected = VerifyCS.Diagnostic("ILA1002").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}
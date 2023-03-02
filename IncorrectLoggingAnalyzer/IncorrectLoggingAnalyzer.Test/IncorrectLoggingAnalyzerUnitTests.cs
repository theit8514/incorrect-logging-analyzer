using System.Threading.Tasks;
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
            const string test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_ReplaceGenericType()
        {
            const string test = @"
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

            const string endResult = @"
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
        public async Task RuleChangeType_ReplaceGenericTypeKeepAttribute()
        {
            const string test = @"
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

            public MyClass([My] ILogger<OtherClass> logger) => _logger = logger;
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
        class MyAttribute : Attribute { }
    }";

            const string endResult = @"
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

            public MyClass([My] ILogger<MyClass> logger) => _logger = logger;
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
        class MyAttribute : Attribute { }
    }";

            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_ReplaceQualifiedType()
        {
            const string test = @"
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

            const string endResult = @"
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
            const string test = @"
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

            const string endResult = @"
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
            const string test = @"
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

            const string endResult = @"
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
        public async Task RuleChangeType_ReplaceBaseType()
        {
            const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly {|#0:ILogger<OtherClass>|} _logger;

            public MyClass(ILogger<OtherClass> logger) : base(logger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            const string endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly ILogger<MyClass> _logger;

            public MyClass(ILogger<MyClass> logger) : base(logger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            // ILogger is covariant, so you can assign the MyClass ILogger to OtherClass as the base class.
            // This should make OtherClass log as MyClass source context.
            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_ReplaceBaseTypeSeparate()
        {
            const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly {|#0:ILogger<OtherClass>|} _logger;

            public MyClass(ILogger<OtherClass> logger) : base(logger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            const string endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly ILogger<MyClass> _logger;

            public MyClass(ILogger<MyClass> logger, ILogger<OtherClass> baseLogger) : base(baseLogger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            // When calling SeparateBaseClass code fix, a new parameter in the constructor is created
            // for the base class' ILogger and used in the base constructor.
            // This should make OtherClass log as OtherClass source context and MyClass log as MyClass source context.
            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult,
                t => { t.CodeActionEquivalenceKey = "CodeFixSeparateBaseClassTitle"; });
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_ReplaceBaseTypeSeparate2()
        {
            const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly {|#0:ILogger<OtherClass>|} _logger;

            public MyClass(object p, ILogger<OtherClass> logger) : base(p, logger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(object p, ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            const string endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly ILogger<MyClass> _logger;

            public MyClass(object p, ILogger<MyClass> logger, ILogger<OtherClass> baseLogger) : base(p, baseLogger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(object p, ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            // When calling SeparateBaseClass code fix, a new parameter in the constructor is created
            // for the base class' ILogger and used in the base constructor.
            // This should make OtherClass log as OtherClass source context and MyClass log as MyClass source context.
            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult,
                t => { t.CodeActionEquivalenceKey = "CodeFixSeparateBaseClassTitle"; });
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_ReplaceBaseTypeSeparate3()
        {
            const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly {|#0:ILogger<OtherClass>|} _logger;

            public MyClass(object baseLogger, ILogger<OtherClass> logger) : base(baseLogger, logger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(object p, ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            const string endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly ILogger<MyClass> _logger;

            public MyClass(object baseLogger, ILogger<MyClass> logger, ILogger<OtherClass> baseLogger1) : base(baseLogger, baseLogger1) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(object p, ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            // When calling SeparateBaseClass code fix, a new parameter in the constructor is created
            // for the base class' ILogger and used in the base constructor.
            // Ensure that the resulting code can compile (for example, if baseLogger identifier is used).
            // This should make OtherClass log as OtherClass source context and MyClass log as MyClass source context.
            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult,
                t => { t.CodeActionEquivalenceKey = "CodeFixSeparateBaseClassTitle"; });
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_ReplaceBaseTypeSeparateKeepAttribute()
        {
            const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly {|#0:ILogger<OtherClass>|} _logger;

            public MyClass(object baseLogger, [My] ILogger<OtherClass> logger) : base(baseLogger, logger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(object p, ILogger<OtherClass> logger) => _logger = logger;
        }
        class MyAttribute : Attribute { }
    }";

            const string endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly ILogger<MyClass> _logger;

            public MyClass(object baseLogger, [My] ILogger<MyClass> logger, ILogger<OtherClass> baseLogger1) : base(baseLogger, baseLogger1) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(object p, ILogger<OtherClass> logger) => _logger = logger;
        }
        class MyAttribute : Attribute { }
    }";

            // When calling SeparateBaseClass code fix, a new parameter in the constructor is created
            // for the base class' ILogger and used in the base constructor.
            // Ensure that the resulting code can compile (for example, if baseLogger identifier is used).
            // This should make OtherClass log as OtherClass source context and MyClass log as MyClass source context.
            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult,
                t => { t.CodeActionEquivalenceKey = "CodeFixSeparateBaseClassTitle"; });
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_WrongBaseClassDoNotRunSeparateCodeFix()
        {
            const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly {|#0:ILogger<OtherClass2>|} _logger;

            public MyClass(ILogger<OtherClass2> logger, ILogger<OtherClass> baseLogger) : base(baseLogger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
        class OtherClass2 {
        }
    }";

            const string endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly {|#0:ILogger<OtherClass2>|} _logger;

            public MyClass(ILogger<OtherClass2> logger, ILogger<OtherClass> baseLogger) : base(baseLogger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
        class OtherClass2 {
        }
    }";

            // Type OtherClass2 is not MyClass' base class, so we should not be able
            // to run the CodeFixSeparateBaseClassTitle code fix.
            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass2", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult, t =>
            {
                t.CodeActionEquivalenceKey = "CodeFixSeparateBaseClassTitle";
                // We are expecting that this diagnostic will remain and not be fixed by the code fix
                t.FixedState.ExpectedDiagnostics.Add(expected);
            });
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_WrongBaseClassRunCodeFix()
        {
            const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly {|#0:ILogger<OtherClass2>|} _logger;

            public MyClass(ILogger<OtherClass2> logger, ILogger<OtherClass> baseLogger) : base(baseLogger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
        class OtherClass2 {
        }
    }";

            const string endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly ILogger<MyClass> _logger;

            public MyClass(ILogger<MyClass> logger, ILogger<OtherClass> baseLogger) : base(baseLogger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
        class OtherClass2 {
        }
    }";

            // Ensure that the CodeFixChangeTypeTitle can explicitly run for the WrongBaseClass test.
            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass2", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult,
                t => { t.CodeActionEquivalenceKey = "CodeFixChangeTypeTitle"; });
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_IgnoresBaseType()
        {
            const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly ILogger<MyClass> _logger;

            public MyClass(ILogger<MyClass> logger, ILogger<OtherClass> baseLogger) : base(baseLogger) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly ILogger<OtherClass> _logger;

            public OtherClass(ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            // In this case, a specific ILogger for OtherClass is assigned to OtherClass,
            // but MyClass uses it's own logger.
            // In this case both OtherClass and MyClass log as separate source contexts.
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_IgnoresBaseTypeConstructor()
        {
            const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly {|#0:ILogger<OtherClass>|} _logger;

            public MyClass(object a, object b, ILogger<OtherClass> logger) : base(a, b) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            public OtherClass(object a, object b) { }
        }
    }";

            const string endResult = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    namespace ConsoleApplication1
    {
        class MyClass : OtherClass
        {
            private readonly ILogger<MyClass> _logger;

            public MyClass(object a, object b, ILogger<MyClass> logger) : base(a, b) {
                _logger = logger;
            }
        }
        class OtherClass
        {
            public OtherClass(object a, object b) { }
        }
    }";

            var expected = VerifyCS.Diagnostic("ILA1001").WithLocation(0)
                .WithArguments("ConsoleApplication1.OtherClass", "ConsoleApplication1.MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, endResult,
                t => t.CodeActionEquivalenceKey = "CodeFixSeparateBaseClassTitle");
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task RuleChangeType_ReplaceKeepsIndentation()
        {
            const string test = @"
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

            public MyClass(object data,
                ILogger<OtherClass> logger)
            {
                _logger = logger;
            }
        }
        class OtherClass
        {
            private readonly Microsoft.Extensions.Logging.ILogger<OtherClass> _logger;

            public OtherClass(Microsoft.Extensions.Logging.ILogger<OtherClass> logger) => _logger = logger;
        }
    }";

            const string endResult = @"
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

            public MyClass(object data,
                ILogger<MyClass> logger)
            {
                _logger = logger;
            }
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
            const string test = @"
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
            const string test = @"
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
            const string test = @"
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
            const string test = @"
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
            const string test = @"
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
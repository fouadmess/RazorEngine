using System;
using RazorEngine;
using RazorEngine.Templating;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string template = @"
@{
    void test(int value)
    {
        for(var i=0; i<=value; i++)
        {
            <div>test: @i</div>
        }
    }    
}

Hello @Model.Name, 
welcome to RazorEngine!

Function with razor:
@{ test(5); }";

            var result = Engine.Razor.RunCompile(template, "templateKey", null, new { Name = "World" });
            Console.WriteLine(result);
        }
    }
}
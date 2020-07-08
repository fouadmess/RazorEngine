// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
namespace Microsoft.AspNetCore.Razor.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Entry point to parse Razor files and generate code.
    /// </summary>
    public class RazorTemplateEngine
    {
        /// <summary>
        /// An isntnace of RazorTemplateEngineOptions
        /// </summary>
        private RazorTemplateEngineOptions options;

        /// <summary>
        /// Gets the <see cref="T:RazorEngine" />.
        /// </summary>
        public RazorEngine Engine { get; }

        /// <summary>
        /// Gets the <see cref="T:RazorProject" />.
        /// </summary>
        public RazorProject Project { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="T:RazorTemplateEngine" />.
        /// </summary>
        /// <param name="engine">The <see cref="T:RazorEngine" />.</param>
        /// <param name="project">The <see cref="T:RazorProject" />.</param>
        public RazorTemplateEngine(RazorEngine engine, RazorProject project)
        {
            this.Engine = engine ?? throw new ArgumentNullException("engine");
            this.Project = project ?? throw new ArgumentNullException("project");
            this.options = new RazorTemplateEngineOptions();
        }

        /// <summary>
        /// Options to configure <see cref="T:RazorTemplateEngine" />.
        /// </summary>
        public RazorTemplateEngineOptions Options
        {
            get
            {
                return this.options;
            }
            set
            {
                this.options = value ?? throw new ArgumentNullException("value");
            }
        }

        /// <summary>
        /// Parses the template specified by the project item <paramref name="path" />.
        /// </summary>
        /// <param name="path">The template path.</param>
        /// <returns>The <see cref="T:RazorCSharpDocument" />.</returns>
        public RazorCSharpDocument GenerateCode(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Argument path Cannot Be Null Or Empty");
            }

            return this.GenerateCode(this.Project.GetItem(path));
        }

        /// <summary>
        /// Parses the template specified by <paramref name="projectItem" />.
        /// </summary>
        /// <param name="projectItem">The <see cref="T:RazorProjectItem" />.</param>
        /// <returns>The <see cref="T:RazorCSharpDocument" />.</returns>
        public RazorCSharpDocument GenerateCode(RazorProjectItem projectItem)
        {
            if (projectItem == null)
            {
                throw new ArgumentNullException("projectItem");
            }

            if (!projectItem.Exists)
            {
                throw new InvalidOperationException($"FormatRazorTemplateEngine_ItemCouldNotBeFound {projectItem.FilePath}");
            }

            return this.GenerateCode(this.CreateCodeDocument(projectItem));
        }

        /// <summary>
        /// Parses the template specified by <paramref name="codeDocument" />.
        /// </summary>
        /// <param name="codeDocument">The <see cref="T:RazorProjectItem" />.</param>
        /// <returns>The <see cref="T:RazorCSharpDocument" />.</returns>
        public virtual RazorCSharpDocument GenerateCode(RazorCodeDocument codeDocument)
        {
            if (codeDocument == null)
            {
                throw new ArgumentNullException("codeDocument");
            }

            this.Engine.Process(codeDocument);
            return codeDocument.GetCSharpDocument();
        }

        /// <summary>
        /// Generates a <see cref="T:RazorCodeDocument" /> for the specified <paramref name="path" />.
        /// </summary>
        /// <param name="path">The template path.</param>
        /// <returns>The created <see cref="T:RazorCodeDocument" />.</returns>
        public virtual RazorCodeDocument CreateCodeDocument(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Argument path Cannot Be Null Or Empty");
            }

            return this.CreateCodeDocument(this.Project.GetItem(path));
        }

        /// <summary>
        /// Generates a <see cref="T:RazorCodeDocument" /> for the specified <paramref name="projectItem" />.
        /// </summary>
        /// <param name="projectItem">The <see cref="T:RazorProjectItem" />.</param>
        /// <returns>The created <see cref="T:RazorCodeDocument" />.</returns>
        public virtual RazorCodeDocument CreateCodeDocument(RazorProjectItem projectItem)
        {
            if (projectItem == null)
            {
                throw new ArgumentNullException("projectItem");
            }

            if (!projectItem.Exists)
            {
                throw new InvalidOperationException($"FormatRazorTemplateEngine_ItemCouldNotBeFound {projectItem.FilePath}");
            }

            var source = RazorSourceDocument.ReadFrom(projectItem);
            var imports = this.GetImports(projectItem);
            return RazorCodeDocument.Create(source, imports);
        }

        /// <summary>
        /// Gets <see cref="T:RazorSourceDocument" /> that are applicable to the specified <paramref name="path" />.
        /// </summary>
        /// <param name="path">The template path.</param>
        /// <returns>The sequence of applicable <see cref="T:RazorSourceDocument" />.</returns>
        public IEnumerable<RazorSourceDocument> GetImports(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Argument path Cannot Be Null Or Empty");
            }

            return this.GetImports(this.Project.GetItem(path));
        }

        /// <summary>
        /// Gets <see cref="T:RazorSourceDocument" /> that are applicable to the specified <paramref name="projectItem" />.
        /// </summary>
        /// <param name="projectItem">The <see cref="T:RazorProjectItem" />.</param>
        /// <returns>The sequence of applicable <see cref="T:RazorSourceDocument" />.</returns>
        public virtual IEnumerable<RazorSourceDocument> GetImports(RazorProjectItem projectItem)
        {
            if (projectItem == null)
            {
                throw new ArgumentNullException("projectItem");
            }

            var list = new List<RazorSourceDocument>();
            foreach (var razorProjectItem in this.GetImportItems(projectItem))
            {
                if (razorProjectItem.Exists)
                {
                    list.Insert(0, RazorSourceDocument.ReadFrom(razorProjectItem));
                }
            }

            if (this.Options.DefaultImports != null)
            {
                list.Insert(0, this.Options.DefaultImports);
            }

            return list;
        }

        /// <summary>
        /// Gets the sequence of imports with the name specified by <see cref="P:RazorTemplateEngineOptions.ImportsFileName" />
        /// that apply to <paramref name="path" />.
        /// </summary>
        /// <param name="path">The path to look up import items for.</param>
        /// <returns>A sequence of <see cref="T:RazorProjectItem" /> instances that apply to the
        /// <paramref name="path" />.</returns>
        public IEnumerable<RazorProjectItem> GetImportItems(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Argument path Cannot Be Null Or Empty");
            }

            return this.GetImportItems(this.Project.GetItem(path));
        }

        /// <summary>
        /// Gets the sequence of imports with the name specified by <see cref="P:RazorTemplateEngineOptions.ImportsFileName" />
        /// that apply to <paramref name="projectItem" />.
        /// </summary>
        /// <param name="projectItem">The <see cref="T:RazorProjectItem" /> to look up import items for.</param>
        /// <returns>A sequence of <see cref="T:RazorProjectItem" /> instances that apply to the
        /// <paramref name="projectItem" />.</returns>
        public virtual IEnumerable<RazorProjectItem> GetImportItems(RazorProjectItem projectItem)
        {
            string importsFileName = this.Options.ImportsFileName;
            if (!string.IsNullOrEmpty(importsFileName))
            {
                return this.Project.FindHierarchicalItems(projectItem.FilePath, importsFileName);
            }

            return Enumerable.Empty<RazorProjectItem>();
        }
    }
}
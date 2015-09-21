﻿using System;
using System.Collections.Generic;
using System.IO;

namespace VirtoCommerce.Web.Views.Engines.Liquid
{
    public class ViewLocationResult
    {
        private string _contents;
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewLocationResult"/> class.
        /// </summary>
        protected ViewLocationResult()
        {

        }

        public ViewLocationResult(IEnumerable<string> searchedLocations)
        {
            SearchedLocations = searchedLocations;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewLocationResult"/> class.
        /// </summary>
        /// <param name="location">The location of where the view was found.</param>
        /// <param name="name">The name of the view.</param>
        /// <param name="contents">A <see cref="TextReader"/> that can be used to read the contents of the located view.</param>
        public ViewLocationResult(string location, string name, Func<TextReader> contents)
        {
            this.Location = location !=null ? location.ToLower() : null;
            this.Name = name != null ? name.ToLower() : null;
            this.ContentsReader = contents;
        }

        /// <summary>
        /// Gets a function that produces a reader for retrieving the contents of the view.
        /// </summary>
        /// <value>A <see cref="Func{T}"/> instance that can be used to produce a reader for retrieving the contents of the view.</value>
        protected Func<TextReader> ContentsReader { get; set; }

        public string Contents
        {
            get
            {
                if ((_contents == null || this.IsStale()) && SearchedLocations == null) // if search location is not null, that means we didn't find file in the first place
                {
                    _contents = ContentsReader.Invoke().ReadToEnd();
                }

                return _contents;
            }
        }

        /// <summary>
        /// Gets the extension of the view that was located.
        /// </summary>
        /// <value>A <see cref="string"/> containing the extension of the view that was located.</value>
        /// <remarks>The extension should not contain a leading dot.</remarks>
        //public string Extension { get; protected set; }

        /// <summary>
        /// Gets the location of where the view was found.
        /// </summary>
        /// <value>A <see cref="string"/> containing the location of the view.</value>
        public string Location { get; protected set; }

        public IEnumerable<string> SearchedLocations { get; protected set; }

        /// <summary>
        /// Gets the full name of the view that was found
        /// </summary>
        /// <value>A <see cref="string"/> containing the name of the view.</value>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the current item is stale
        /// </summary>
        /// <returns>True if stale, false otherwise</returns>
        public virtual bool IsStale()
        {
            return false;
        }
    }
}

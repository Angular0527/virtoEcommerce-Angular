﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.PowerShell.Utilities
{
    internal class DatabaseName
    {
        public static DatabaseName Parse(string name)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var parts = name.Trim().Split(new[] { '.' });

            Contract.Assert(parts.Length > 0);

            if (parts.Length > 2)
            {
                throw new ArgumentException("Invalid database name " + name);
            }

            string schema = null;
            string objectName;

            if (parts.Length == 2)
            {
                schema = parts[0];

                if (string.IsNullOrWhiteSpace(schema))
                {
                    throw new ArgumentException("Invalid database name " + name);
                }

                objectName = parts[1];
            }
            else
            {
                objectName = parts[0];
            }

            if (string.IsNullOrWhiteSpace(objectName))
            {
                throw new ArgumentException("Invalid database name " + name);
            }

            return new DatabaseName(objectName, schema);
        }

        // Note: This class is currently immutable. If you make it mutable then you
        // must ensure that instances are cloned when cloning the DbModelBuilder.
        private readonly string _name;
        private readonly string _schema;

        public DatabaseName(string name)
            : this(name, null)
        {
        }

        public DatabaseName(string name, string schema)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            _name = name;
            _schema = schema;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Schema
        {
            get { return _schema; }
        }

        public override string ToString()
        {
            var s = _name;

            if (_schema != null)
            {
                s = _schema + "." + s;
            }

            return s;
        }

        public bool Equals(DatabaseName other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(other._name, _name, StringComparison.Ordinal)
                   && string.Equals(other._schema, _schema, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return (obj.GetType() == typeof(DatabaseName))
                   && Equals((DatabaseName)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_name.GetHashCode() * 397) ^ (_schema != null ? _schema.GetHashCode() : 0);
            }
        }
    }
}

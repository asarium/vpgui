using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using VPGUI.Models;
using VPSharp.Entries;
using IDataObject = System.Windows.IDataObject;

namespace VPGUI.Utilities
{
    [ComVisible(true)]
    public class VPDataObject : IDataObject
    {
        private readonly Dictionary<string, object> _dataEntries;
        private readonly MainModel _mainModel;
        private IEnumerable<VPEntry> _vpDataEntries;

        public VPDataObject(IEnumerable<VPEntry> vpDataEntries, MainModel model)
        {
            Contract.Assert(vpDataEntries != null);

            this._vpDataEntries = vpDataEntries;
            _mainModel = model;

            _dataEntries = new Dictionary<string, object>();
        }

        #region IDataObject Members

        public object GetData(string format)
        {
            return GetData(format, true);
        }

        public object GetData(Type format)
        {
            return GetData(format.FullName, true);
        }

        public object GetData(string format, bool autoConvert)
        {
            if (!autoConvert)
            {
                object data;

                if (_dataEntries.TryGetValue(format, out data))
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                object data;

                if (_dataEntries.TryGetValue(format, out data))
                {
                    return data;
                }
                else
                {
                    if (format == DataFormats.FileDrop)
                    {
                        var result = _mainModel.ExtractEntriesAsync(null, _vpDataEntries).Result.ToArray();
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public bool GetDataPresent(string format)
        {
            return GetDataPresent(format, true);
        }

        public bool GetDataPresent(Type format)
        {
            return GetDataPresent(format.FullName, true);
        }

        public bool GetDataPresent(string format, bool autoConvert)
        {
            if (_dataEntries.ContainsKey(format))
            {
                return true;
            }
            else
            {
                if (format == DataFormats.FileDrop && this._vpDataEntries.Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string[] GetFormats()
        {
            return GetFormats(true);
        }

        public string[] GetFormats(bool autoConvert)
        {
            int size = _dataEntries.Keys.Count;

            if (autoConvert && this._vpDataEntries.Any())
            {
                size++;
            }

            var formats = new List<string>(size);
            formats.AddRange(this._dataEntries.Keys);

            if (autoConvert && this._vpDataEntries.Any())
            {
                formats.Add(DataFormats.FileDrop);
            }

            return formats.ToArray();
        }

        public void SetData(object data)
        {
            if (data == null)
            {
                throw new InvalidOperationException();
            }
            else
            {
                SetData(data.GetType(), data);
            }
        }

        public void SetData(string format, object data)
        {
            SetData(format, data, true);
        }

        public void SetData(Type format, object data)
        {
            SetData(format.FullName, data, true);
        }

        public void SetData(string format, object data, bool autoConvert)
        {
            if (format == DataFormats.FileDrop)
            {
                throw new InvalidOperationException();
            }

            _dataEntries[format] = data;
        }

        #endregion
    }
}

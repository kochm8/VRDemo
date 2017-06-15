using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace CpvrLab.VirtualTable
{

    /// <summary>
    /// Generic score board that can hold any kind of table data and sync over the network
    /// 
    /// note:   As a first approach I tried handling the entire serialization in OnSerialize/OnDeserialize
    ///         however the data will reach the limit of 1400 bytes set by unity pretty fast.
    ///         A good solution for serializing and sending large data http://answers.unity3d.com/questions/1113376/unet-send-big-amount-of-data-over-network-how-to-s.html
    ///         However the solution in the link isnt applicable for our problem. Since we don't want to
    ///         send the entire score board every time. But rather send incremental updates.
    ///         So the current form of this class is a poor version of sending those incremental updates.
    ///         I already spent too much time on this so this is how it has to be for now.
    /// </summary>
    public class ScoreBoard : NetworkBehaviour
    {
        public Action<ScoreBoard> OnDataChanged;
        public Action OnShow;
        public Action OnHide;

        private bool _dataChanged = false; // dirty flag for event listeners
        private bool _newClient = false; // flag telling us if a new client connected and needs data

        private string _title = "";
        private bool _show = false;
        private List<string> _headers = new List<string>();
        private List<List<string>> _rowData = new List<List<string>>();
        private List<float> _cellSizeRatios = new List<float>(); // todo

        public string title { get { return _title; } }
        public bool show { get { return _show; } }
        public List<string> headers { get { return _headers; } }
        public List<List<string>> rowData { get { return _rowData; } }
        public int rows { get { return _rowData.Count; } }
        public int cols { get { return headers.Count; } }

        [Server]
        public void SetTitle(string title)
        {
            _title = title;
            RpcUpdateTitle(title);
            _dataChanged = true;
        }

        [Server]
        public void Show(bool val)
        {
            ShowLocal(val);
            RpcShow(val);
        }

        [Server]
        public void SetHeaders(string[] data)
        {
            _headers = new List<string>(data);
            RpcUpdateHeaders(Serialize(_headers));
            _dataChanged = true;
        }

        [Server]
        public void AddRow(string[] data)
        {
            var row = new List<string>(data);
            _rowData.Add(row);
            RpcAddRow(Serialize(row));
            _dataChanged = true;
        }

        [Server]
        public void SetRowData(int rowNum, string[] data)
        {
            _rowData[rowNum] = new List<string>(data);
            RpcUpdateRow(rowNum, Serialize(_rowData[rowNum]));
            _dataChanged = true;
        }

        [Server]
        public void SetCellData(int rowNum, int colNum, string data)
        {
            _rowData[rowNum][colNum] = data;
            RpcUpdateCellData(rowNum, colNum, data);
            _dataChanged = true;
        }

        [Server]
        public void ClearData()
        {
            _rowData.Clear();
            _dataChanged = true;
            RpcClearData();
        }

        byte[] Serialize(List<string> list)
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();

            binFormatter.Serialize(mStream, list);
            return mStream.ToArray();
        }

        List<string> Deserialize(byte[] data)
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            mStream.Write(data, 0, data.Length);
            mStream.Position = 0;

            return binFormatter.Deserialize(mStream) as List<string>;
        }
        [ClientRpc]
        void RpcUpdateTitle(string title)
        {
            if (isServer) return;

            _title = title;
            _dataChanged = true;
        }

        [ClientRpc]
        void RpcInitScoreboard(int colCount, int rowCount)
        {
            if (isServer) return;
            
            if (cols == colCount && rows == rowCount)
                return;

            // initialize the arrays with the new dimensions
            _headers.Clear();
            for (int i = 0; i < colCount; i++)
                _headers.Add("");

            _rowData.Clear();
            for (int i = 0; i < rowCount; i++)
            {
                var row = new List<string>();
                _rowData.Add(row);
                for (int j = 0; j < colCount; j++)
                    row.Add("");
            }
        }

        [ClientRpc]
        void RpcAddRow(byte[] data)
        {
            if (isServer) return;
            
            _rowData.Add(Deserialize(data));
            _dataChanged = true;
        }
        [ClientRpc]
        void RpcUpdateRow(int index, byte[] data)
        {
            if (isServer) return;
            
            _rowData[index] = Deserialize(data);
            _dataChanged = true;
        }
        [ClientRpc]
        void RpcUpdateCellData(int rowNum, int colNum, string data)
        {
            if (isServer) return;
            
            _rowData[rowNum][colNum] = data;
            _dataChanged = true;
        }
        [ClientRpc]
        void RpcUpdateHeaders(byte[] data)
        {
            if (isServer) return;
            
            _headers = Deserialize(data);
            _dataChanged = true;
        }
        [ClientRpc]
        void RpcClearData()
        {
            if (isServer) return;
            
            _rowData.Clear();
            _dataChanged = true;
        }

        [ClientRpc]
        void RpcShow(bool val)
        {
            ShowLocal(val);
        }

        private void ShowLocal(bool val)
        {
            _show = val;
            if (_show)
            {
                if (OnShow != null)
                    OnShow();
            }
            else
            {
                if (OnHide != null)
                    OnHide();
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                // always write initial state, no dirty bits
            }
            else if (syncVarDirtyBits == 0)
            {
                writer.WritePackedUInt32(0);
                return false;
            }
            else
            {
                // dirty bits
                writer.WritePackedUInt32(1);
            }

            // we only end up here if a new client connects and needs the current scoreboard data
            // so we'll send over the entire thing
            _newClient = true;

            return true;
        }

        // the approach below didn't work because the data is too large
        // but I'll keep it around for later
        //public override bool OnSerialize(NetworkWriter writer, bool initialState)
        //{
        //    if (initialState)
        //    {
        //        // always write initial state, no dirty bits
        //    }
        //    else if (syncVarDirtyBits == 0)
        //    {
        //        writer.WritePackedUInt32(0);
        //        return false;
        //    }
        //    else
        //    {
        //        // dirty bits
        //        writer.WritePackedUInt32(1);
        //    }

        //    Debug.Log("Serializing");            

        //    writer.Write(_title);
        //    writer.Write(_rows);
        //    writer.Write(_cols);

        //    var binFormatter = new BinaryFormatter();
        //    var mStream = new MemoryStream();

        //    // serialize headers
        //    binFormatter.Serialize(mStream, _headers);
        //    var byteArray = mStream.ToArray();            
        //    writer.WriteBytesAndSize(byteArray, byteArray.Length);

        //    // serialize rows
        //    for (int i = 0; i < _rows; i++)
        //    {
        //        binFormatter.Serialize(mStream, _rowData[i]);
        //        byteArray = mStream.ToArray();
        //        writer.WriteBytesAndSize(byteArray, byteArray.Length);
        //    }

        //    return true;
        //}

        //public override void OnDeserialize(NetworkReader reader, bool initialState)
        //{
        //    if (isServer && NetworkServer.localClientActive)
        //        return;

        //    if (!initialState)
        //    {
        //        if (reader.ReadPackedUInt32() == 0)
        //            return;
        //    }

        //    Debug.Log("Deserializing");

        //    _title = reader.ReadString();
        //    _rows = reader.ReadInt32();
        //    _cols = reader.ReadInt32();

        //    var mStream = new MemoryStream();
        //    var binFormatter = new BinaryFormatter();

        //    // deserialize headers
        //    byte[] objectBytes = reader.ReadBytesAndSize();            
        //    mStream.Write(objectBytes, 0, objectBytes.Length);
        //    mStream.Position = 0;
        //    var test = binFormatter.Deserialize(mStream) as List<string>;

        //    // deserialize rows
        //    _rowData.Clear();
        //    for (int i = 0; i < _rows; i++)
        //    {
        //        objectBytes = reader.ReadBytesAndSize();
        //        mStream.Write(objectBytes, 0, objectBytes.Length);
        //        mStream.Position = 0;
        //        var row = binFormatter.Deserialize(mStream) as List<string>;
        //        _rowData.Add(row);
        //    }

        //    _dataChanged = true;
        //}

        void Update()
        {

            // TEMP TEST
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    // run update only on the server
            //    if (!isServer)
            //        return;

            //    SetTitle("test " + UnityEngine.Random.Range(0, 100).ToString());
            //    Debug.Log("Server: Changing title to " + _title);

            //    _headers.Clear();
            //    _headers.Add("player");
            //    _headers.Add("rounds won");
            //    _headers.Add("best time");

            //    var row = new List<string>();
            //    row.Add("wacki");
            //    row.Add("2");
            //    row.Add("0.33");

            //    AddRow(row.ToArray());

            //    row = new List<string>();
            //    row.Add("noob");
            //    row.Add("1");
            //    row.Add("5.13");

            //    AddRow(row.ToArray());       
            //}

            if (_dataChanged)
            {
                if (OnDataChanged != null)
                    OnDataChanged(this);

                _dataChanged = false;
            }

            if (isServer && _newClient)
            {
                RpcInitScoreboard(cols, rows);
                RpcUpdateTitle(_title);
                RpcUpdateHeaders(Serialize(_headers));
                for (int i = 0; i < rows; i++)
                {
                    RpcUpdateRow(i, Serialize(_rowData[i]));
                }

                _newClient = false;
            }
        }
    }

}
﻿// Copyright 2013 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Serilog.Sinks.DumpFile
{
    class DumpFileSink : ILogEventSink
    {
        readonly TextWriter _output;
        readonly object _syncRoot = new object();

        public DumpFileSink(string path)
        {
            TryCreateDirectory(path);
            _output = new StreamWriter(File.OpenWrite(path));
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");

            lock (_syncRoot)
            {
                _output.Write("[" + logEvent.Timestamp + "] " + logEvent.Level + ": \"");
                _output.Write(logEvent.MessageTemplate);
                _output.WriteLine("\"");
                if (logEvent.Exception != null)
                    _output.WriteLine(logEvent.Exception);
                foreach (var property in logEvent.Properties)
                {
                    _output.Write(property.Key + " = ");
                    property.Value.Render(_output);
                    _output.WriteLine();
                }
                _output.WriteLine();
                _output.Flush();
            }
        }

        static void TryCreateDirectory(string path)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Failed to create directory {0}: {1}", path, ex);
            }
        }
    }
}

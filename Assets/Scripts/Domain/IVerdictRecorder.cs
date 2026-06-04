using System;
using System.Collections.Generic;
using CriminalCase2.Data;

namespace CriminalCase2.Domain
{
    public interface IVerdictRecorder
    {
        IReadOnlyList<VerdictRecord> Records { get; }
        event Action<VerdictRecord>? VerdictRecorded;

        void Record(SuspectData suspect, SuspectRole role);
    }
}

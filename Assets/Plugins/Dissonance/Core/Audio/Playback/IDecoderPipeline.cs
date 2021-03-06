using System;
using NAudio.Wave;

namespace Dissonance.Audio.Playback
{
    internal interface IDecoderPipeline
    {
        /// <summary>
        /// Number of buffers waiting for playback
        /// </summary>
        int BufferCount { get; }

        /// <summary>
        /// Total amount of time which is in buffers
        /// </summary>
        TimeSpan BufferTime { get; }

        /// <summary>
        /// Packet loss detected in playback (0-1)
        /// </summary>
        float PacketLoss { get; }

        /// <summary>
        /// Get the amount of time contained in a single frame at the input into this pipeline
        /// </summary>
        TimeSpan InputFrameTime { get; }

        PlaybackOptions PlaybackOptions { get; }

        [NotNull] WaveFormat OutputFormat { get; }

        void Prepare(SessionContext context);
        bool Read(ArraySegment<float> samples);
    }
}
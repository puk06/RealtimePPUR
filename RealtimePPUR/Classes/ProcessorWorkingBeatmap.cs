// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Skinning;
using System.IO;

namespace RealtimePPUR.Classes
{
    public class ProcessorWorkingBeatmap(IBeatmap beatmap) : WorkingBeatmap(beatmap.BeatmapInfo, null)
    {
        public ProcessorWorkingBeatmap(string file)
            : this(ReadFromFile(file))
        {
        }

        private static Beatmap ReadFromFile(string filename)
        {
            using var stream = File.OpenRead(filename);
            using var reader = new LineBufferedReader(stream);
            return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
        }

        public static ProcessorWorkingBeatmap FromFile(string file) => new(file);

        protected override IBeatmap GetBeatmap() => beatmap;
        public override Texture GetBackground() => null;
        protected override Track GetBeatmapTrack() => null;
        protected override ISkin GetSkin() => null;
        public override Stream GetStream(string storagePath) => null;
    }
}

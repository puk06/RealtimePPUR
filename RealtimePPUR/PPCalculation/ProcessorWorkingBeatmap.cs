// Copyright(c) 2019 ppy Pty Ltd <contact@ppy.sh>.
// This code is borrowed from osu-tools(https://github.com/ppy/osu-tools)
// osu-tools is licensed under the MIT License. https://github.com/ppy/osu-tools/blob/master/LICENCE

using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Skinning;

namespace RealtimePPUR.PPCalculation;

public class ProcessorWorkingBeatmap(IBeatmap beatmap) : WorkingBeatmap(beatmap.BeatmapInfo, null)
{
    public ProcessorWorkingBeatmap(string file) : this(ReadFromFile(file))
    {
    }

    public static ProcessorWorkingBeatmap FromFile(string file)
        => new(file);

    private static Beatmap ReadFromFile(string filename)
    {
        using var stream = File.OpenRead(filename);
        using var reader = new LineBufferedReader(stream);
        return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
    }

    protected override IBeatmap GetBeatmap() => beatmap;
    public override Texture GetBackground() => null!;
    protected override Track GetBeatmapTrack() => null!;
    protected override ISkin GetSkin() => null!;
    public override Stream GetStream(string storagePath) => null!;
}

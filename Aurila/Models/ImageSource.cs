using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Models;

public abstract record ImageSource
{
}

public record UrlImageSource(string Url) : ImageSource;

public record ByteArrayImageSource(byte[] Data, ImageFormat Format) : ImageSource;

public record StreamImageSource(Stream Data, ImageFormat Format) : ImageSource;

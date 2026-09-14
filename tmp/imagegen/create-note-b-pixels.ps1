Add-Type -AssemblyName System.Drawing

$outputDirectory = Join-Path (Get-Location) 'output/imagegen/notes'
$previewDirectory = Join-Path (Get-Location) 'tmp/imagegen'
[System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null

$spriteWidth = 88
$spriteHeight = 12
$palette = @{
    Outline = '#091724'
    Bevel = '#52788B'
    InnerRim = '#102839'
    LowerRim = '#173D54'
    FaceTop = '#FFFCF4'
    FaceBottom = '#F5F2E9'
}
$variants = @(
    @{ Name = 'NoteB_Left'; Main = '#00C9EE'; Light = '#90EEFF'; Dark = '#008EAD' },
    @{ Name = 'NoteB_Right'; Main = '#FFE33C'; Light = '#FFF6A1'; Dark = '#C9AC16' }
)

function Set-PixelSpan {
    param($Bitmap, [int]$Row, [int]$First, [int]$Last, [string]$Hex)
    $color = [System.Drawing.ColorTranslator]::FromHtml($Hex)
    for ($column = $First; $column -le $Last; $column++) {
        $Bitmap.SetPixel($column, $Row, $color)
    }
}

foreach ($variant in $variants) {
    $bitmap = [System.Drawing.Bitmap]::new(
        $spriteWidth, $spriteHeight,
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        # Transparent corners, with opaque pixels touching each canvas edge.
        $insets = @(3, 2, 1, 0, 0, 0, 0, 0, 0, 1, 2, 3)
        for ($row = 0; $row -lt $spriteHeight; $row++) {
            Set-PixelSpan $bitmap $row $insets[$row] ($spriteWidth - 1 - $insets[$row]) $palette.Outline
        }

        Set-PixelSpan $bitmap 1 3 84 $palette.Bevel
        Set-PixelSpan $bitmap 2 2 85 $palette.InnerRim
        Set-PixelSpan $bitmap 9 2 85 $palette.InnerRim
        Set-PixelSpan $bitmap 10 3 84 $palette.LowerRim

        for ($row = 3; $row -le 8; $row++) {
            $faceInset = 8
            if ($row -eq 3 -or $row -eq 8) { $faceInset = 9 }
            $faceColor = $palette.FaceTop
            if ($row -ge 7) { $faceColor = $palette.FaceBottom }
            Set-PixelSpan $bitmap $row $faceInset ($spriteWidth - 1 - $faceInset) $faceColor
        }

        # Mirror the endcaps so both lane sprites share the exact same geometry.
        $capRows = @(
            @(1, 3, 7, 'Light'),
            @(2, 2, 7, 'Main'),
            @(3, 1, 6, 'Main'),
            @(4, 1, 5, 'Main'),
            @(5, 1, 5, 'Main'),
            @(6, 1, 5, 'Main'),
            @(7, 1, 5, 'Main'),
            @(8, 1, 6, 'Main'),
            @(9, 2, 7, 'Main'),
            @(10, 3, 7, 'Dark')
        )
        foreach ($cap in $capRows) {
            Set-PixelSpan $bitmap $cap[0] $cap[1] $cap[2] $variant[$cap[3]]
            Set-PixelSpan $bitmap $cap[0] ($spriteWidth - 1 - $cap[2]) ($spriteWidth - 1 - $cap[1]) $variant[$cap[3]]
        }

        $filePath = Join-Path $outputDirectory ($variant.Name + '.png')
        $bitmap.Save($filePath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $bitmap.Dispose() }
}

# An enlarged inspection image is separate from the native-resolution exports.
$preview = [System.Drawing.Bitmap]::new(736, 272)
try {
    for ($y = 0; $y -lt $preview.Height; $y++) {
        for ($x = 0; $x -lt $preview.Width; $x++) {
            $shade = 40
            if (([int][Math]::Floor($x / 8) + [int][Math]::Floor($y / 8)) % 2 -eq 0) { $shade = 58 }
            $preview.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($shade, $shade, $shade))
        }
    }
    for ($index = 0; $index -lt $variants.Count; $index++) {
        $sprite = [System.Drawing.Bitmap]::new((Join-Path $outputDirectory ($variants[$index].Name + '.png')))
        try {
            for ($y = 0; $y -lt $sprite.Height; $y++) {
                for ($x = 0; $x -lt $sprite.Width; $x++) {
                    $pixel = $sprite.GetPixel($x, $y)
                    if ($pixel.A -eq 0) { continue }
                    for ($dy = 0; $dy -lt 8; $dy++) {
                        for ($dx = 0; $dx -lt 8; $dx++) {
                            $preview.SetPixel(16 + $x * 8 + $dx, 24 + $index * 128 + $y * 8 + $dy, $pixel)
                        }
                    }
                }
            }
        }
        finally { $sprite.Dispose() }
    }
    $preview.Save((Join-Path $previewDirectory 'note-b-preview.png'), [System.Drawing.Imaging.ImageFormat]::Png)
}
finally { $preview.Dispose() }

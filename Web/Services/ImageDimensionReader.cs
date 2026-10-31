namespace Web.Services;

// Lit les dimensions d'une image PNG/JPEG/WebP/GIF directement depuis les octets d'en-tete,
// sans decoder l'image entiere - aucune dependance (native ou commerciale) necessaire.
// Extrait de CartesController (meme logique, reutilisee telle quelle par tout controleur qui
// a besoin de valider des dimensions d'image avant stockage - ex. ArticlesAdminController).
public static class ImageDimensionReader
{
    public static (int Largeur, int Hauteur)? LireDimensions(byte[] octets)
    {
        if (octets.Length >= 24 && octets[0] == 0x89 && octets[1] == 0x50 && octets[2] == 0x4E && octets[3] == 0x47)
        {
            // PNG : signature (8 octets) puis chunk IHDR (largeur/hauteur en big-endian, 4 octets chacun).
            var largeur = (octets[16] << 24) | (octets[17] << 16) | (octets[18] << 8) | octets[19];
            var hauteur = (octets[20] << 24) | (octets[21] << 16) | (octets[22] << 8) | octets[23];
            return (largeur, hauteur);
        }

        if (octets.Length >= 10 && octets[0] == 0x47 && octets[1] == 0x49 && octets[2] == 0x46)
        {
            // GIF : signature "GIF87a"/"GIF89a" (6 octets) puis largeur/hauteur en little-endian, 2 octets chacun.
            var largeur = octets[6] | (octets[7] << 8);
            var hauteur = octets[8] | (octets[9] << 8);
            return (largeur, hauteur);
        }

        if (octets.Length >= 21 && octets[0] == 0x52 && octets[1] == 0x49 && octets[2] == 0x46 && octets[3] == 0x46
            && octets[8] == 0x57 && octets[9] == 0x45 && octets[10] == 0x42 && octets[11] == 0x50)
        {
            return LireDimensionsWebp(octets);
        }

        if (octets.Length >= 2 && octets[0] == 0xFF && octets[1] == 0xD8)
        {
            return LireDimensionsJpeg(octets);
        }

        return null;
    }

    private static (int Largeur, int Hauteur)? LireDimensionsWebp(byte[] octets)
    {
        var fourCc = System.Text.Encoding.ASCII.GetString(octets, 12, 4);
        switch (fourCc)
        {
            case "VP8X":
                // Chunk VP8X : flags (1 octet) + reserve (3 octets), puis largeur-1/hauteur-1 sur
                // 3 octets little-endian chacun.
                if (octets.Length < 30)
                {
                    return null;
                }
                var largeurX = (octets[24] | (octets[25] << 8) | (octets[26] << 16)) + 1;
                var hauteurX = (octets[27] | (octets[28] << 8) | (octets[29] << 16)) + 1;
                return (largeurX, hauteurX);
            case "VP8 ":
                // Bitstream VP8 "lossy" : tag de frame (3 octets) + code de synchronisation
                // 0x9D 0x01 0x2A, puis largeur/hauteur sur 14 bits chacun (les 2 bits de poids
                // fort servent a l'echelle et sont ignores).
                if (octets.Length < 30 || octets[23] != 0x9D || octets[24] != 0x01 || octets[25] != 0x2A)
                {
                    return null;
                }
                var largeurL = (octets[26] | (octets[27] << 8)) & 0x3FFF;
                var hauteurL = (octets[28] | (octets[29] << 8)) & 0x3FFF;
                return (largeurL, hauteurL);
            case "VP8L":
                // Lossless : signature 0x2F puis largeur-1/hauteur-1 sur 14 bits chacun, empaquetes
                // sur 4 octets little-endian.
                if (octets.Length < 25 || octets[20] != 0x2F)
                {
                    return null;
                }
                var bits = octets[21] | (octets[22] << 8) | (octets[23] << 16) | (octets[24] << 24);
                var largeurLL = (bits & 0x3FFF) + 1;
                var hauteurLL = ((bits >> 14) & 0x3FFF) + 1;
                return (largeurLL, hauteurLL);
            default:
                return null;
        }
    }

    private static (int Largeur, int Hauteur)? LireDimensionsJpeg(byte[] octets)
    {
        var position = 2; // apres le marqueur SOI (0xFFD8)
        while (position + 4 <= octets.Length)
        {
            if (octets[position] != 0xFF)
            {
                position++;
                continue;
            }

            // Octets de bourrage 0xFF eventuels avant le vrai marqueur.
            while (position + 1 < octets.Length && octets[position + 1] == 0xFF)
            {
                position++;
            }

            var marqueur = octets[position + 1];
            position += 2;

            // Marqueurs sans segment de longueur (TEM, RST0-RST7, SOI, EOI).
            if (marqueur == 0x01 || (marqueur >= 0xD0 && marqueur <= 0xD7) || marqueur == 0xD8 || marqueur == 0xD9)
            {
                continue;
            }

            if (position + 2 > octets.Length)
            {
                return null;
            }

            var longueurSegment = (octets[position] << 8) | octets[position + 1];

            // Marqueurs SOFn (debut de frame) : 0xC0-0xCF sauf 0xC4 (DHT), 0xC8 (reserve), 0xCC (DAC).
            var estSof = marqueur >= 0xC0 && marqueur <= 0xCF && marqueur != 0xC4 && marqueur != 0xC8 && marqueur != 0xCC;
            if (estSof)
            {
                if (position + 7 > octets.Length)
                {
                    return null;
                }
                var hauteur = (octets[position + 3] << 8) | octets[position + 4];
                var largeur = (octets[position + 5] << 8) | octets[position + 6];
                return (largeur, hauteur);
            }

            if (marqueur == 0xDA)
            {
                // Debut du flux de donnees scanne : aucun marqueur SOF trouve avant.
                return null;
            }

            position += longueurSegment;
        }

        return null;
    }
}

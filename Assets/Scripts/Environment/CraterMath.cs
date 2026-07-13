using UnityEngine;

public static class CraterMath
{
    public struct TunnelData
    {
        public Vector3 center;
        public Quaternion rotation;
        public Vector3 extents;
    }

    private static TunnelData[] tunnels;

    public static void InitializeTunnels()
    {
        if (tunnels != null) return;
        
        tunnels = new TunnelData[1];
        
        // Tunnel 1: Miniera 
        // Orientato a metà tra Nord-Ovest (135°) e Nord (90°) = 112.5°
        float angleDeg = 112.5f; 
        float angleRad = angleDeg * Mathf.Deg2Rad;
        
        // Il "secondo terrazzamento" dal fondo si trova a y = -125m.
        // La parte piatta di questo terrazzamento va da circa R=175 a R=210.
        // Da R=210 inizia il muro che sale al terzo terrazzamento.
        // Facciamo iniziare il tunnel a R=205 (così poggia perfettamente sul piano) e lo facciamo 
        // entrare nella montagna fino a R=325.
        float centerR = 265f; 
        
        // Pendenza in discesa: da y=-123 a y=-135 (dislivello di 12m su lunghezza 120m)
        float pitchDeg = Mathf.Atan2(12f, 120f) * Mathf.Rad2Deg;

        tunnels[0] = new TunnelData {
            center = new Vector3(Mathf.Cos(angleRad) * centerR, -129f, Mathf.Sin(angleRad) * centerR),
            // Rotazione Yaw: -angleDeg + 90 allinea l'asse Z locale con il raggio verso l'esterno
            rotation = Quaternion.Euler(pitchDeg, -angleDeg + 90f, 0f), 
            extents = new Vector3(18f, 13f, 60f) // Larghezza 36m, Altezza 26m, Lunghezza 120m
        };
    }

    public static bool IntersectsTunnel(Vector3 wPos, float size)
    {
        InitializeTunnels();
        float radius = size * 0.866f; // Circoscrizione di un cubo
        for (int i=0; i<tunnels.Length; i++) {
            if (Vector3.Distance(wPos, tunnels[i].center) < tunnels[i].extents.magnitude + radius) {
                return true;
            }
        }
        return false;
    }

    private static float GetArchDistance(Vector3 point, TunnelData tunnel)
    {
        Vector3 p = Quaternion.Inverse(tunnel.rotation) * (point - tunnel.center);
        float W = tunnel.extents.x;
        float H = tunnel.extents.y;
        float L = tunnel.extents.z;
        
        // Calcoliamo il centro del semicerchio superiore in modo che la cima tocchi esattamente +H
        float archCenterY = H - W; 
        
        float x = Mathf.Abs(p.x);
        float dist2D;
        
        if (p.y > archCenterY) {
            // Parte superiore: Semicerchio
            dist2D = new Vector2(x, p.y - archCenterY).magnitude - W;
        } else {
            // Parte inferiore: Muri dritti e pavimento piatto
            float dX = x - W;
            float dY = -H - p.y; // Distanza dal pavimento
            
            if (dX > 0 && dY > 0) {
                dist2D = new Vector2(dX, dY).magnitude; // Esterno all'angolo in basso
            } else {
                dist2D = Mathf.Max(dX, dY); // Interno, o esterno lungo i lati
            }
        }
        
        // Estrusione 3D lungo l'asse Z (Lunghezza del tunnel)
        Vector2 d3D = new Vector2(dist2D, Mathf.Abs(p.z) - L);
        float outsideDist = Vector2.Max(d3D, Vector2.zero).magnitude;
        float insideDist = Mathf.Min(Mathf.Max(d3D.x, d3D.y), 0f);
        
        return outsideDist + insideDist;
    }

    // Restituisce l'altezza della superficie del cratere in un dato punto (x, z)
    public static float GetSurfaceHeight(float x, float z, float maxRadius = 600f)
    {
        float currentRadius = Mathf.Sqrt(x * x + z * z);
        float angle = Mathf.Atan2(z, x);
        if (angle < 0) angle += Mathf.PI * 2f;

        float nx = Mathf.Cos(angle);
        float nz = Mathf.Sin(angle);

        // Rumore di Perlin applicato radialmente
        float noiseOuter = (Mathf.PerlinNoise(nx * 1.5f + 10f, nz * 1.5f + 10f) - 0.5f) * 225f;
        float noiseMid = (Mathf.PerlinNoise(nx * 2.5f + 20f, nz * 2.5f + 20f) - 0.5f) * 150f;
        float noiseInner = (Mathf.PerlinNoise(nx * 1.0f + 30f, nz * 1.0f + 30f) - 0.5f) * 120f;

        float outerBound = maxRadius + noiseOuter;
        float midBound = (maxRadius * 0.7f) + noiseMid;
        float innerBound = (maxRadius * 0.15f) + noiseInner;

        innerBound = Mathf.Clamp(innerBound, 10f, midBound - 20f);
        midBound = Mathf.Clamp(midBound, innerBound + 20f, outerBound - 20f);

        float posY = 0f;

        if (currentRadius <= innerBound)
        {
            posY = -400f; // Downtown
        }
        else if (currentRadius <= midBound)
        {
            float t = (currentRadius - innerBound) / (midBound - innerBound);
            
            float tDistorted;
            if (t < 0.25f) {
                tDistorted = (t / 0.25f) * 0.5f;
            } else {
                tDistorted = 0.5f + ((t - 0.25f) / 0.75f) * 0.5f;
            }
            
            float stepC = Mathf.Floor(tDistorted * 4f);
            float fracC = (tDistorted * 4f) - stepC;
            float terraceTC = fracC < 0.3f ? 0f : (fracC - 0.3f) / 0.7f;
            terraceTC = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(terraceTC));
            float finalTC = (stepC + terraceTC) / 4f;
            finalTC = Mathf.Lerp(tDistorted, finalTC, 0.85f);
            
            float spiralOffset = (angle / (Mathf.PI * 2f)) * 0.25f;
            float tSpiral = tDistorted + spiralOffset;
            float stepS = Mathf.Floor(tSpiral * 4f);
            float fracS = (tSpiral * 4f) - stepS;
            float terraceTS = fracS < 0.6f ? 0f : (fracS - 0.6f) / 0.4f;
            terraceTS = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(terraceTS));
            float finalTS = (stepS + terraceTS) / 4f - spiralOffset;
            finalTS = Mathf.Lerp(tDistorted, finalTS, 0.85f);
            
            float blend = Mathf.SmoothStep(0.35f, 0.65f, tDistorted);
            float finalT = Mathf.Lerp(finalTC, finalTS, blend);

            float edgeFade = Mathf.SmoothStep(0f, 1f, t * 15f) * Mathf.SmoothStep(0f, 1f, (1f - t) * 15f);
            finalT = Mathf.Lerp(tDistorted, finalT, edgeFade);

            posY = Mathf.Lerp(-400f, 150f, finalT);
        }
        else
        {
            posY = 150f; // Residenziale
        }

        float surfaceNoiseAmp = 0f;
        if (currentRadius > innerBound && currentRadius <= midBound) surfaceNoiseAmp = 0.5f;
        else if (currentRadius > midBound) surfaceNoiseAmp = 2.5f;

        float surfaceNoise = (Mathf.PerlinNoise(nx * currentRadius * 0.05f, nz * currentRadius * 0.05f) - 0.5f) * surfaceNoiseAmp;
        posY += surfaceNoise;

        // Assicuriamoci che non buchi mai il fondo
        if (posY < -400f) posY = -400f;

        return posY;
    }

    // Calcola la densità Voxel (Roccia > 0, Aria < 0)
    public static float GetDensity(float x, float y, float z)
    {
        float surfaceHeight = GetSurfaceHeight(x, z);
        
        // Densità base del cratere
        float density = surfaceHeight - y;

        // Isola Zenit (Isola fluttuante gigante al centro del cielo)
        if (x > -250f && x < 250f && z > -250f && z < 250f && y > 100f && y < 450f)
        {
            float topY = 350f;
            
            // Densità base negativa di sicurezza
            float islandDensity = -1000f;

            // 1. FORMA BASE COMPLESSA (3 Coni Sovrapposti e Scentrati)
            // L'isola non è più un cerchio perfetto, ma la fusione di 3 grandi blocchi
            // Formato: { OffsetX, OffsetZ, Profondità, Raggio }
            float[,] cones = new float[,] {
                { 20f, -10f, 200f, 110f },   // Cono 1 (Il principale, enorme)
                { -50f, 40f, 130f, 80f },    // Cono 2 (Grande circa la metà, a Nord-Ovest)
                { -20f, -60f, 110f, 70f }    // Cono 3 (Più piccolo, a Sud)
            };

            for (int i = 0; i < cones.GetLength(0); i++) 
            {
                float cDist = Vector2.Distance(new Vector2(x, z), new Vector2(cones[i, 0], cones[i, 1]));
                float cRad = cones[i, 3];
                
                float cBottom;
                if (cDist < cRad) {
                    float dropCurve = Mathf.Pow(1f - cDist / cRad, 1.3f);
                    cBottom = topY - cones[i, 2] * dropCurve;
                } else {
                    // Gradiente continuo per evitare i "cubetti di Minecraft" sui bordi!
                    // Più ci si allontana, più il fondo sale e sparisce nel nulla
                    cBottom = topY + (cDist - cRad) * 2f; 
                }
                
                float cDensity = Mathf.Min(topY - y, y - cBottom);
                
                // Operazione Booleana: Max() unisce i tre coni in un'unica massa solida e compatta
                if (cDensity > islandDensity) islandDensity = cDensity;
            }

            // 2. RADICI SECONDARIE (Spuntoni aggiuntivi per accentuare il sottomondo)
            float[,] roots = new float[,] {
                { 70f, 45f, 150f, 30f },   
                { -40f, -50f, 120f, 25f }
            };

            for (int i = 0; i < roots.GetLength(0); i++) 
            {
                float rDist = Vector2.Distance(new Vector2(x, z), new Vector2(roots[i, 0], roots[i, 1]));
                float rRad = roots[i, 3];
                
                float rBottom;
                if (rDist < rRad) {
                    float rDrop = Mathf.Pow(1f - rDist / rRad, 1.5f); // 1.5 rende le radici molto affilate
                    rBottom = topY - roots[i, 2] * rDrop;
                } else {
                    rBottom = topY + (rDist - rRad) * 3f;
                }
                
                float rDensity = Mathf.Min(topY - y, y - rBottom);
                if (rDensity > islandDensity) islandDensity = rDensity;
            }

            // 3. GRANDI VOLUMI ORGANICI (3D fBm Noise)
            // Modella i fianchi dei coni creando pance in fuori e profonde caverne in dentro
            float freq = 0.03f;
            float n1 = Mathf.PerlinNoise(x * freq, y * freq) - 0.5f;
            float n2 = Mathf.PerlinNoise(y * freq, z * freq) - 0.5f;
            float n3 = Mathf.PerlinNoise(z * freq, x * freq) - 0.5f;
            float baseWarp = (n1 + n2 + n3) * 20f; 

            // 4. DETTAGLI ROCCIOSI E SPIGOLI (Ridged Noise)
            // Crea creste affilate come rasoi per dare l'illusione di vere spaccature rocciose
            float dFreq = 0.07f;
            float r1 = Mathf.Abs(Mathf.PerlinNoise(x * dFreq, y * dFreq) - 0.5f);
            float r2 = Mathf.Abs(Mathf.PerlinNoise(y * dFreq, z * dFreq) - 0.5f);
            float r3 = Mathf.Abs(Mathf.PerlinNoise(z * dFreq, x * dFreq) - 0.5f);
            float ridgeNoise = (0.5f - (r1 + r2 + r3) / 3f) * 30f; 

            // Applichiamo il rumore alla geometria
            islandDensity += baseWarp;
            islandDensity += ridgeNoise;

            // 5. TAGLIO NETTO DEL PLATEAU
            islandDensity = Mathf.Min(islandDensity, topY - y);

            // Materializza l'isola nel mondo Voxel
            if (islandDensity > density)
            {
                density = islandDensity;
            }
        }

        // --- SOTTRAZIONE TUNNEL (Operazione Booleana) ---
        // Sottraiamo i tunnel dalla roccia per scavare gallerie artificiali
        if (tunnels != null)
        {
            Vector3 point = new Vector3(x, y, z);
            for (int i = 0; i < tunnels.Length; i++)
            {
                float tunnelDist = GetArchDistance(point, tunnels[i]);
                // La distanza è < 0 DENTRO il tunnel.
                // Facendo il Min, forziamo la densità ad essere negativa all'interno del tunnel, creando "Aria".
                // Questo crea pareti lisce e perfettamente dritte.
                density = Mathf.Min(density, tunnelDist);
            }
        }

        return density;
    }
}

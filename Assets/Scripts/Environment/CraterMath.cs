using UnityEngine;

public static class CraterMath
{
    public enum TunnelType { Arch, Room }

    public struct TunnelData
    {
        public TunnelType type;
        public Vector3 center;
        public Quaternion rotation;
        public Vector3 extents;
        // Parametri organici
        public float snakeAmplitude;
        public float snakeFrequency;
        public float noiseAmplitude;
        public bool hasSharpCurves;
        public bool dynamicPitchBend;
    }

    private static TunnelData[] tunnels;

    public static void InitializeTunnels()
    {
        if (tunnels != null) return;
        
        tunnels = new TunnelData[5];
        
        float mainAngle = 112.5f; 
        float mainRad = mainAngle * Mathf.Deg2Rad;
        Vector3 mainDir = new Vector3(Mathf.Cos(mainRad), 0f, Mathf.Sin(mainRad));
        
        // Quota ingresso (terrazzamento reale calcolato ad occhio)
        float entranceY = -135f; 
        // Quota stanza (più profonda per permettere la discesa)
        float roomFloorY = -155f; 
        
        // --- 2. STANZA PRINCIPALE DELLA MINIERA ---
        float roomCenterR = 425f;
        Vector3 roomCenter = new Vector3(mainDir.x * roomCenterR, roomFloorY + 20f, mainDir.z * roomCenterR);
        tunnels[1] = new TunnelData {
            type = TunnelType.Room,
            center = roomCenter,
            rotation = Quaternion.Euler(0f, -mainAngle + 90f, 0f), 
            extents = new Vector3(45f, 20f, 45f), // Raggio 45m
            snakeAmplitude = 0f,
            snakeFrequency = 0f,
            noiseAmplitude = 8f 
        };

        // --- 1. TUNNEL PRINCIPALE (Ingresso) ---
        float t1Length = 200f;
        // La distanza dal pivot all'ingresso è t1Length - 5f = 195f.
        // Vogliamo che su 195m il tunnel scenda (da entranceY a roomFloorY).
        float t1Pitch = Mathf.Atan2(Mathf.Abs(entranceY - roomFloorY), 195f) * Mathf.Rad2Deg; 
        Quaternion t1Rot = Quaternion.Euler(t1Pitch, -mainAngle + 90f, 0f);
        Vector3 t1LocalZ = t1Rot * Vector3.forward;
        // Punto di ancoraggio: al limite della stanza
        Vector3 t1Pivot = new Vector3(roomCenter.x, roomFloorY + 13f, roomCenter.z) - mainDir * 40f;
        tunnels[0] = new TunnelData {
            type = TunnelType.Arch,
            center = t1Pivot - t1LocalZ * (t1Length / 2f - 5f),
            rotation = t1Rot, 
            extents = new Vector3(18f, 13f, t1Length / 2f), 
            snakeAmplitude = 6f,
            snakeFrequency = 0.05f,
            noiseAmplitude = 2.5f,
            hasSharpCurves = false,
            dynamicPitchBend = false
        };

        // Funzione helper per calcolare il centro di un cunicolo in uscita dalla stanza
        Vector3 GetBranchCenter(float yawAngle, float pitchAngle, float length, float extentsY) {
            Quaternion rot = Quaternion.Euler(pitchAngle, -yawAngle + 90f, 0f);
            Vector3 localZ = rot * Vector3.forward; 
            Vector3 dir = new Vector3(Mathf.Cos(yawAngle * Mathf.Deg2Rad), 0f, Mathf.Sin(yawAngle * Mathf.Deg2Rad));
            // Punto di ancoraggio al limite della stanza (raggio ~40m)
            Vector3 pivot = new Vector3(roomCenter.x, roomFloorY + extentsY, roomCenter.z) + dir * 40f;
            return pivot + localZ * (length / 2f - 5f);
        }

        // --- 3. CUNICOLO A (Sinistra) - Curve a gomito ---
        float angleA = mainAngle + 40f; 
        float pitchA = 7f; // Discesa
        float lengthA = 200f;
        tunnels[2] = new TunnelData {
            type = TunnelType.Arch,
            center = GetBranchCenter(angleA, pitchA, lengthA, 7f),
            rotation = Quaternion.Euler(pitchA, -angleA + 90f, 0f), 
            extents = new Vector3(6f, 7f, lengthA / 2f), 
            snakeAmplitude = 0f, // Gestito dalle curve a gomito
            snakeFrequency = 0f,
            noiseAmplitude = 2.0f,
            hasSharpCurves = true,
            dynamicPitchBend = false
        };

        // --- 4. CUNICOLO B (Destra) ---
        float angleB = mainAngle - 35f; 
        float pitchB = -5f; // Salita
        float lengthB = 250f;
        tunnels[3] = new TunnelData {
            type = TunnelType.Arch,
            center = GetBranchCenter(angleB, pitchB, lengthB, 6f),
            rotation = Quaternion.Euler(pitchB, -angleB + 90f, 0f), 
            extents = new Vector3(5f, 6f, lengthB / 2f), 
            snakeAmplitude = 12f, // Curve morbide
            snakeFrequency = 0.04f,
            noiseAmplitude = 2.5f,
            hasSharpCurves = false,
            dynamicPitchBend = false
        };

        // --- 5. CUNICOLO C (Dritto profondo) - Cambio pendenza ---
        float pitchC = 12f; // Inizia con forte discesa
        float lengthC = 300f;
        tunnels[4] = new TunnelData {
            type = TunnelType.Arch,
            center = GetBranchCenter(mainAngle, pitchC, lengthC, 5f),
            rotation = Quaternion.Euler(pitchC, -mainAngle + 90f, 0f), 
            extents = new Vector3(4f, 5f, lengthC / 2f), 
            snakeAmplitude = 5f,
            snakeFrequency = 0.02f,
            noiseAmplitude = 1.5f,
            hasSharpCurves = false,
            dynamicPitchBend = true // Piana improvvisamente a metà
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

    private static float GetTunnelDistance(Vector3 point, TunnelData tunnel)
    {
        Vector3 p = Quaternion.Inverse(tunnel.rotation) * (point - tunnel.center);
        
        // --- DEFORMAZIONE ORGANICA (Snaking) ---
        if (tunnel.snakeAmplitude > 0f) {
            float turn = Mathf.PerlinNoise(p.z * tunnel.snakeFrequency, Mathf.Abs(tunnel.center.x) * 0.01f) * 2f - 1f;
            p.x += turn * turn * turn * tunnel.snakeAmplitude;
        }

        // --- CURVE A GOMITO STRETTE ---
        if (tunnel.hasSharpCurves) {
            float length = tunnel.extents.z;
            // Addolciamo leggermente i moltiplicatori per evitare che l'algoritmo Voxel 
            // perdi la continuità della mesh a causa di una distorsione spaziale troppo violenta.
            float shift1 = Mathf.Atan((p.z - (-length * 0.2f)) * 0.08f) * 15f; // Prima curva stretta
            float shift2 = Mathf.Atan((p.z - (length * 0.4f)) * 0.1f) * -20f;  // Seconda curva a gomito
            p.x += shift1 + shift2;
        }

        // --- CAMBIO DI PENDENZA DINAMICO ---
        if (tunnel.dynamicPitchBend) {
            // A metà del tunnel, pieghiamo la coordinata Y per far "spianare" la discesa
            // Mathf.SmoothStep garantisce che il pavimento non abbia alcuno scalino durante la piegatura
            float bendFactor = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(p.z / (tunnel.extents.z * 0.6f)));
            float pitchUp = 12f * Mathf.Deg2Rad; // Annulliamo i 12 gradi di discesa iniziali
            p.y -= Mathf.Sin(pitchUp) * p.z * bendFactor;
        }

        float W = tunnel.extents.x;
        float H = tunnel.extents.y;
        float L = tunnel.extents.z;
        
        float finalDist;

        if (tunnel.type == TunnelType.Room) {
            // --- STANZA POLIGONALE IRREGOLARE ---
            float angle = Mathf.Atan2(p.z, p.x);
            // Costruiamo la base di un pentagono 
            float pentagonBase = Mathf.Cos(Mathf.PI / 5f - (Mathf.Abs(angle) % (Mathf.PI * 2f / 5f))); 
            float deformedW = W / pentagonBase;
            
            // Aggiungiamo rumore Perlin a bassa frequenza per curvare alcune pareti
            float noise = Mathf.PerlinNoise(point.x * 0.04f, point.z * 0.04f) * 12f;
            deformedW += noise;
            
            float domeDist = new Vector3(p.x, Mathf.Max(p.y, 0f), p.z).magnitude - deformedW;
            float floorDist = -H - p.y;
            // Il pavimento è SEMPRE piatto e non affetto dalla deformazione radiale!
            finalDist = Mathf.Max(domeDist, floorDist);
        } 
        else {
            // --- TUNNEL AD ARCO ---
            float archCenterY = H - W; 
            float x = Mathf.Abs(p.x);
            float dist2D;
            
            if (p.y > archCenterY) {
                dist2D = new Vector2(x, p.y - archCenterY).magnitude - W;
            } else {
                float dX = x - W;
                float dY = -H - p.y; 
                
                if (dX > 0 && dY > 0) {
                    dist2D = new Vector2(dX, dY).magnitude; 
                } else {
                    dist2D = Mathf.Max(dX, dY); 
                }
            }
            
            Vector2 d3D = new Vector2(dist2D, Mathf.Abs(p.z) - L);
            float outsideDist = Vector2.Max(d3D, Vector2.zero).magnitude;
            float insideDist = Mathf.Min(Mathf.Max(d3D.x, d3D.y), 0f);
            finalDist = outsideDist + insideDist;
        }

        // --- RUMORE ROCCIOSO (Pareti grezze) ---
        if (tunnel.noiseAmplitude > 0f) {
            // Maschera del pavimento: il rumore non deve distruggere la strada!
            // Inizia a zero sul pavimento (-H) e arriva al 100% a 3 metri di altezza
            float floorMask = Mathf.Clamp01((p.y - (-H + 0.5f)) / 3.0f);
            
            if (floorMask > 0f) {
                float nf = 0.12f; 
                float noise = (Mathf.PerlinNoise(point.x * nf, point.z * nf) - 0.5f) 
                            + (Mathf.PerlinNoise(point.y * nf, point.x * nf) - 0.5f);
                
                // Sottrarre dalla distanza equivale a scavare roccia in modo irregolare
                finalDist -= noise * tunnel.noiseAmplitude * floorMask;
            }
        }
        
        return finalDist;
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
            posY += Mathf.PerlinNoise(nx * 3f, nz * 3f) * 15f;
            posY += Mathf.PerlinNoise(nx * 10f, nz * 10f) * 5f;
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

        // --- SOTTRAZIONE TUNNEL E STANZE (Operazione Booleana) ---
        // Sottraiamo i tunnel dalla roccia per scavare gallerie artificiali
        if (tunnels != null)
        {
            Vector3 point = new Vector3(x, y, z);
            for (int i = 0; i < tunnels.Length; i++)
            {
                float tunnelDist = GetTunnelDistance(point, tunnels[i]);
                density = Mathf.Min(density, tunnelDist);
            }
        }

        return density;
    }
}

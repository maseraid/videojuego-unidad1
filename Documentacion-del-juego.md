# El Piso es Lava: documentación completa del juego

Este documento explica **todo el juego tal como está ahora**: la idea, la escena, los objetos y sus componentes, los 6 scripts, la física y las colisiones, los sprites, las animaciones, la interfaz, la iluminación, el ciclo de una partida y dónde se cambia cada valor.

---

## Índice
1. [El juego en pocas palabras](#1-el-juego-en-pocas-palabras)
2. [Estructura del proyecto](#2-estructura-del-proyecto)
3. [La escena `10_Nivel`](#3-la-escena-10_nivel)
4. [Los prefabs](#4-los-prefabs)
5. [Los scripts](#5-los-scripts)
6. [Física y colisiones](#6-física-y-colisiones)
7. [Sprites y arte](#7-sprites-y-arte)
8. [Animaciones](#8-animaciones)
9. [Interfaz (HUD)](#9-interfaz-hud)
10. [Iluminación](#10-iluminación)
11. [Una partida de principio a fin](#11-una-partida-de-principio-a-fin)
12. [Dónde se cambia cada valor](#12-dónde-se-cambia-cada-valor)
13. [Ajustes del proyecto](#13-ajustes-del-proyecto)

---

## 1. El juego en pocas palabras

El caballero empieza abajo y tiene que **subir saltando de plataforma en plataforma** hasta la **puerta** de arriba, antes de que lo alcance la **lava**, que sube cada vez más rápido.

- **Un solo intento:** si la lava te toca o te caes por debajo de la cámara, pierdes.
- **Dos tipos de plataforma:**
  - de **hierba**, que se atraviesan desde abajo y se pisan desde arriba;
  - **rompibles** (bloques agrietados), que al pisarlas se ponen naranjas, desaparecen y al rato vuelven.
- **Se gana** tocando la puerta. El juego muestra el tiempo que tardaste.

### Controles
| Acción | Teclado | Mando |
|---|---|---|
| Moverse | A / D o ← → | Stick izquierdo o cruceta |
| Saltar (mantener = más alto) | Espacio | Botón A (sur) |
| Bajar de una plataforma | S (o ↓) + Espacio | Abajo + A |
| Pausa | Esc | Start |
| Repetir (en pausa, al ganar o al perder) | R | — |
| Elegir botón en los paneles | Flechas + Enter, o ratón | Cruceta + A |

---

## 2. Estructura del proyecto

```
videojuegoU1/
├── Assets/
│   ├── _Project/                        TODO lo propio del juego
│   │   ├── Scenes/10_Nivel.unity        la única escena
│   │   ├── Scripts/                     los 6 scripts (sección 5)
│   │   ├── Prefabs/                     Jugador, Lava, Plataforma, PlataformaRompible
│   │   ├── Art/
│   │   │   ├── Characters/              caballero.png + Animaciones/ (controlador y 4 clips)
│   │   │   ├── Lava/                    lava_superficie.png, lava_cuerpo.png
│   │   │   ├── Nivel/                   pared_izq, pared_der, suelo, plataforma, rompible
│   │   │   └── UI/                      barra.png (barra de lava)
│   │   └── Data/SinFriccion.physicsMaterial2D
│   ├── Nature_pixel_art_assets/         paquete: de aquí salen paredes, suelo y hierba
│   ├── PixelPack03_Free/                paquete: el bloque agrietado
│   ├── Medieval_pixel_art_asset_FREE/   paquete: la puerta de la meta
│   ├── TooCubeForest/                   paquete: el fondo de montañas
│   ├── Recursos-20261012/               la fuente ARCADECLASSIC
│   ├── Settings/                        ajustes de render URP (2D)
│   └── TutorialInfo/, Welcome/          solo quedan imágenes de plantilla (no se usan)
├── Packages/                            paquetes de Unity (sección 13)
└── ProjectSettings/                     capas, física, nombre del juego, escenas...
```

**Paquetes de Unity:**
| Paquete | Para qué |
|---|---|
| Input System | Los controles (teclado, mando y ratón en los botones) |
| Universal RP | El render 2D y las luces |
| UI (uGUI) | El HUD: textos, imágenes y botones |
| 2D Sprite | El editor de sprites (cortar hojas de sprites) |
| Visual Studio | Editar el código desde VS / VS Code |

---

## 3. La escena `10_Nivel`

Todo el nivel está **colocado a mano** en esta escena. Lo que ves en el editor es lo que se juega.

### Medidas
- **1 bloque = 1 unidad de Unity.**
- **Ancho total:** 40 bloques.
  - Paredes de 8 bloques a cada lado.
  - Zona de juego de 24 bloques (x = 8 a 32).
- **Alto:** unos 160 bloques.
  - Suelo de y = 0 a 3.
  - La puerta está a unos 148.
- **El jugador empieza** en x = 11,5, y = 3, de pie sobre el suelo.

### Objetos de la escena y sus componentes
| Objeto | Componentes | Qué es |
|---|---|---|
| **Main Camera** (tag *MainCamera*) | Camera (ortográfica, tamaño 11,25), Universal Additional Camera Data, Audio Listener, **CameraOnlyUp** | La cámara que solo sube y mueve el fondo |
| **Luz global 2D** | Light2D (global, intensidad 0,9) | Ilumina todo el nivel por igual |
| **Fondo** | SpriteRenderer | La imagen de montañas, detrás de todo |
| **Jugador** | *(prefab, ver sección 4)* | El caballero |
| **Lava** | *(prefab, ver sección 4)* | La lava que sube |
| **Meta** (tag ***Finish***) | BoxCollider2D (*Is Trigger*) | La zona de la puerta que da la victoria |
| ↳ **Puerta** | SpriteRenderer | El dibujo de la puerta |
| ↳ **Luz meta** | Light2D (verde, radio 3) | Brillo para verla de lejos |
| **Pared izquierda / Pared derecha / Suelo** (capa *Ground*) | SpriteRenderer (modo *Tiled*) + BoxCollider2D | Los límites macizos del nivel |
| **Plataformas** | — (solo agrupa) | Contiene las **61 plataformas**: 41 de hierba y 20 rompibles, copias de los prefabs |
| **GameManager** | **GameManager** | Las reglas de la partida y la lava |
| **HUD** | Canvas, Canvas Scaler, Graphic Raycaster, **HUD** | La interfaz (sección 9) |
| **EventSystem** | Event System, Input System UI Input Module | Para que los botones respondan a ratón, teclado y mando |

---

## 4. Los prefabs

Un **prefab** es un objeto guardado como plantilla. Las copias en la escena siguen enlazadas a él: si cambias el prefab, cambian todas.

### `Jugador`
| Objeto | Componentes | Detalles |
|---|---|---|
| **Jugador** (capa *Player*, tag *Player*) | **Rigidbody2D** | Dinámico, rotación congelada, interpolado, detección de colisión continua. La gravedad la ajusta el código |
| | **CapsuleCollider2D** | Vertical, 0,7 × 1,3, desplazado 0,65 hacia arriba (el punto de referencia está en los pies). Material **SinFriccion** (fricción 0) |
| | **PlayerController** | El script del movimiento |
| ↳ **Sprite** | **SpriteRenderer** + **Animator** | El dibujo del caballero (orden 20) y su controlador de animaciones `caballero.controller` |

### `Lava`
| Objeto | Componentes | Detalles |
|---|---|---|
| **Lava** (capa *Lava*) | solo Transform | Su posición *y* es la superficie de la lava. **No tiene collider**: la altura la compara el GameManager |
| ↳ **Superficie** | SpriteRenderer (*Tiled*, orden 51) | La ola de arriba, repetida a lo ancho |
| ↳ **Cuerpo** | SpriteRenderer (*Tiled*, orden 50) | El interior, 40 bloques hacia abajo |
| ↳ **Luz lava 1, 2, 3** | Light2D | Luces naranjas (intensidad 1,4, radio 9) |

### `Plataforma` (hierba)
| Componente | Detalles |
|---|---|
| **SpriteRenderer** | Modo *Tiled* con `plataforma.png`; el ancho se cambia en *Size* |
| **BoxCollider2D** | Mismo tamaño que el dibujo (0,27 de alto); **Used By Effector** marcado |
| **PlatformEffector2D** | *Use One Way* ✔, *Surface Arc* 160°, sin fricción ni rebote laterales (ver 6.3) |
| Capa | **OneWay** |

### `PlataformaRompible`
Lo mismo que *Plataforma* (con `rompible.png` y 1 bloque de alto), más el script **CrumblingPlatform**.

---

## 5. Los scripts

Hay **6 scripts** (unas 830 líneas), todos en `Assets/_Project/Scripts/` y en el *namespace* `PisoEsLava`.

```
                 ┌──────────────┐  lee los controles   ┌──────────────────┐
                 │  GameInput   │◄─────────────────────│ PlayerController │──► Animator
                 │  (estático)  │◄───────┐             └────────┬─────────┘
                 └──────────────┘        │      LocksInput,     │ PlayerReachedGoal()
                                         │      Die(), Freeze() ▼
┌──────────────┐  BottomY        ┌──────┴───────┐  eventos StateChanged / PauseChanged  ┌──────┐
│ CameraOnlyUp │◄────────────────│ GameManager  │──────────────────────────────────────►│ HUD  │
│ (+ fondo)    │                 │ (+ lava)     │◄──── botones (Continue, Reload, Quit) ─│      │
└──────────────┘                 └──────────────┘                                        └──────┘
                                                      CrumblingPlatform: independiente (cada plataforma rompible)
```

### 5.1 `GameInput` (Core): los controles
- **Qué hace:** crea por código las acciones del **Input System**, sin usar ningún archivo `.inputactions`.
- **Acciones:**
  - **Mover:** WASD, flechas, stick izquierdo y cruceta.
  - **Saltar:** Espacio y botón A.
  - **Pausa:** Esc y Start.
  - **Reintentar:** R.
- **Qué ofrece a los demás scripts:** `Move` (vector), `JumpPressed`, `JumpHeld`, `JumpReleased`, `PausePressed` y `RetryPressed`.
- **Detalles:**
  - Se crea la primera vez que alguien lo usa (`Init()`).
  - `ResetStatics()` lo reinicia si Unity entra en Play sin recargar los scripts.

### 5.2 `GameManager` (Core): el árbitro de la partida
**Máquina de estados:**
```
 Countdown ──(acaba la cuenta)──► Playing ──(tocas la puerta)──► Won
     │                               │
     └──(lava o caída)──► Lost ◄─────┘
```
| Estado | Qué pasa cada fotograma |
|---|---|
| **Countdown** | Resta la cuenta atrás (3 s). La lava está quieta y el jugador no se puede mover (`LocksInput`) |
| **Playing** | Suma el **tiempo de juego**, **mueve la lava** (`MoveLava`) y comprueba el peligro (`CheckDanger`) |
| **Won / Lost** | Nada se mueve; la tecla R repite |

**La lava** (`MoveLava`):
```csharp
float speed = baseSpeed + acceleration * (Time.time - lavaStartTime); // cada vez más rápida
if (LavaGap > maxGap)      speed *= catchUpMultiplier;  // te alejaste mucho: te alcanza (x1,8)
else if (LavaGap < minGap) speed *= mercyMultiplier;    // está muy cerca: te da un respiro (x0,7)
lava.position += Vector3.up * speed * Time.deltaTime;
```
`LavaGap` es la distancia entre tus pies y la superficie de la lava. También la usa la barra del HUD.

**Perder** (`CheckDanger`, luego `Damage`):
- pierdes si `jugador.y < lava.y` (la lava te alcanzó) o si `jugador.y < cámara.BottomY − 1,5` (te caíste);
- entonces se guarda el motivo, se llama a `player.Die()` y el estado pasa a **Lost**.

**Ganar** (`PlayerReachedGoal`, que llama el jugador): `player.Freeze()` y el estado pasa a **Won**.

**Pausa** (`SetPaused`): `Time.timeScale = 0`, que detiene física, animaciones y tiempos.

**Botones:**
- `Continue()`: quita la pausa.
- `Reload()`: vuelve a cargar la escena.
- `Quit()`: en el editor para el Play; en el juego compilado lo cierra.

**Comunicación:**
- `GameManager.Instance` permite a otros scripts encontrarlo.
- Los eventos `StateChanged` y `PauseChanged` avisan al HUD (patrón **observador**).

### 5.3 `PlayerController` (Player): el caballero
| Parte | Cómo funciona |
|---|---|
| **Correr** | En `FixedUpdate`, la velocidad horizontal se acerca a `moveSpeed` (7) con aceleración 80 en el suelo y 50 en el aire (`Mathf.MoveTowards`) |
| **Salto** | Velocidad inicial `v = √(2·g·h)`: con g = 30 y h = 3,5 da **14,49 bloques/s** |
| **Salto variable** | Si sueltas el botón mientras sube, la velocidad baja a la del salto mínimo (1,2 bloques) |
| **Caída más rápida** | Al bajar, la gravedad se multiplica por 1,6. Velocidad máxima de caída: 20 |
| **Coyote time** (0,1 s) | Puedes saltar un instante después de salir del borde |
| **Jump buffer** (0,12 s) | Si pulsas saltar justo antes de tocar el suelo, el salto se hace igual |
| **Detectar el suelo** | `Physics2D.BoxCast` bajo los pies contra las capas Ground y OneWay. Si sube, no cuenta como apoyado |
| **Bajar de una plataforma** | S + Espacio sobre una de capa OneWay: `Physics2D.IgnoreCollision` con ella durante 0,3 s |
| **Cuenta atrás y pausa** | Con `LocksInput` no se mueve (la gravedad sí). En pausa no lee los controles |
| **Meta** | `OnTriggerEnter2D`: si lo que toca tiene el tag **Finish**, llama a `GameManager.PlayerReachedGoal()` |
| **Perder** (`Die`) | Desactiva el control, lo deja quieto (Rigidbody *Kinematic*), apaga su collider y lo pone **rojizo** (`HurtColor`) |
| **Ganar** (`Freeze`) | Desactiva el control y lo deja quieto |
| **Animaciones** | `UpdateAnimator()` envía 3 parámetros al Animator (sección 8). `flipX` lo gira hacia donde camina |

### 5.4 `CameraOnlyUp` (Camera): cámara y fondo
```csharp
float desired = Mathf.Clamp(target.position.y + verticalOffset, minY, maxY); // 3 sobre el jugador, con límites
highestY = Mathf.Max(highestY, desired);                                     // nunca baja
baseY = Mathf.SmoothDamp(baseY, highestY, ref velocity, smoothTime);         // la alcanza suavemente
transform.position = new Vector3(x, baseY, z);
background.position = backgroundStart + Vector3.up * ((baseY - startY) * backgroundFactor); // paralaje
```
- Corre en `LateUpdate`, después de que el jugador se mueva.
- **Límites:** *Min Y* 11,25 y *Max Y* 148,75.
- **Paralaje:** el fondo sube el **84 %** de lo que sube la cámara, así que se queda un poco atrás y parece lejano.
- `BottomY` es el borde de abajo de lo que se ve. Lo usa el GameManager para saber si te caíste.

### 5.5 `CrumblingPlatform` (Level): plataformas que se rompen
1. `OnCollisionStay2D`: solo reacciona si el que la toca es el jugador **y está encima**, con los pies a la altura de la parte de arriba.
2. Luego, una corrutina (`Crumble`) hace tres pasos:
   - **aviso:** pone el sprite en `warningColor` (naranja) y espera `warningTime` (1 s);
   - **desaparece:** apaga el collider y el dibujo, y espera `respawnTime` (3 s);
   - **vuelve:** color normal, dibujo y collider encendidos.
3. Comprueba `enabled` porque Unity llama a los métodos de choque aunque el script esté desactivado.

### 5.6 `HUD` (UI): la interfaz
- **Cuenta atrás:** muestra 3, 2, 1 y debajo `introText`; al acabar, «YA!» durante 0,8 s.
- **Barra de lava:** `lavaBar.fillAmount = 1 − distancia/15`. Está vacía si la lava está lejos y llena si está encima.
- **Paneles:** se suscribe a los eventos del GameManager y muestra cada panel con sus botones:

| Cuándo | Título | Texto | Botones |
|---|---|---|---|
| Pausa | PAUSA | — | Continuar, Repetir nivel, Salir |
| Ganar | LLEGASTE ARRIBA (+ tu frase) | TIEMPO *n* SEGUNDOS | Repetir nivel, Salir |
| Perder por la lava | HAS MUERTO QUEMADITO! | INTENTALO OTRA VEZ | Repetir nivel, Salir |
| Perder por caerse | TE CAISTE QUE PENA! | INTENTALO OTRA VEZ | Repetir nivel, Salir |

- `ShowPanel` deja seleccionado el primer botón, para poder usar las flechas y Enter o el mando.

---

## 6. Física y colisiones

### 6.1 Capas
| Capa | Nº | Objetos | Choca con |
|---|---|---|---|
| Player | 6 | Jugador | Ground, OneWay (y Default, donde está la Meta) |
| Ground | 7 | Paredes y suelo | Player |
| OneWay | 8 | Plataformas de hierba y rompibles | Player |
| Lava | 12 | Lava (sin collider) | — |

La matriz de choques está en *Project Settings → Physics 2D*. El código busca las capas **por su nombre** (`LayerMask.GetMask("Ground")`).

### 6.2 Qué colisiona y cómo
| Situación | Mecanismo |
|---|---|
| Caminar, chocar con paredes, aterrizar | Rigidbody2D + CapsuleCollider2D del jugador contra BoxCollider2D de paredes, suelo y plataformas |
| Saber si está en el suelo | `BoxCast` desde los pies, solo contra Ground y OneWay |
| Atravesar plataformas desde abajo | **PlatformEffector2D** de las plataformas |
| Bajar de una plataforma | `Physics2D.IgnoreCollision` durante 0,3 s |
| Pisar una rompible | `OnCollisionStay2D` en `CrumblingPlatform` |
| Llegar a la meta | Collider *trigger* + tag **Finish** → `OnTriggerEnter2D` del jugador |
| Tocar la lava | Sin colisión: el GameManager compara alturas |
| Caerse | Sin colisión: el GameManager compara con el borde de la cámara |

### 6.3 El Platform Effector 2D
Hace que un collider sea **de un solo sentido**. Requiere marcar **Used By Effector** en el collider.

| Propiedad | Valor | Para qué |
|---|---|---|
| Use One Way | ✔ | Solo es sólido por un lado |
| Surface Arc | 160° | Solo cuentan los choques que vienen desde arriba (±80°). Desde abajo y los lados se atraviesa. No es 180° para que no te enganches en las esquinas |
| Rotational Offset | 0 | «Arriba» es arriba |
| Use Side Friction / Bounce | ✘ / ✘ | Sin rozamiento ni rebote en los bordes |

**Consecuencia útil:** al atravesar una rompible desde abajo **no se rompe**, porque el efector ignora ese choque y `OnCollisionStay2D` no se llama.

### 6.4 Material sin fricción
`SinFriccion` (fricción 0, rebote 0) en la cápsula del jugador. Sirve para que **no se quede pegado a una pared** si salta apretado contra ella.

---

## 7. Sprites y arte

Todo el pixel art usa filtro **Point** (sin suavizado) y **sin compresión**, para que los píxeles se vean nítidos.
- **PPU** (*Pixels Per Unit*) dice cuántos píxeles ocupan 1 bloque.
- El modo **Tiled** del SpriteRenderer **repite** el dibujo para llenar un tamaño dado. Así se estiran paredes, suelo, plataformas y lava sin deformarlas.

| Archivo | Tamaño | PPU | Usado en | Origen |
|---|---|---|---|---|
| `Art/Characters/caballero.png` | 448 × 64 (cuadros de 32 × 32, hoja *Multiple*) | 16 | Caballero | propio |
| `Art/Nivel/pared_izq.png` / `pared_der.png` | 384 × 48 | 48 | Paredes (*Tiled*) | bloques de **Nature** |
| `Art/Nivel/suelo.png` | 48 × 144 | 48 | Suelo (*Tiled*) | bloques de **Nature** |
| `Art/Nivel/plataforma.png` | 48 × 13 | 48 | Plataformas de hierba (*Tiled*) | bloque 116 de **Nature** |
| `Art/Nivel/rompible.png` | 16 × 16 | 16 | Plataformas rompibles (*Tiled*) | `Blocks_2` de **PixelPack03** |
| `Art/Lava/lava_superficie.png` / `lava_cuerpo.png` | 48 × 48 | 48 | Lava (*Tiled*) | propio |
| `Art/UI/barra.png` | 4 × 4 blanco | 4 | Barra de lava (*Filled*) | propio |
| `Medieval_props_free.png` (`Medieval_props_free_2`) | hoja *Multiple* | 48 | Puerta (escala 1,25) | **Medieval** |
| `BG_mountains_sunset_1024.png` | 1024 × 1024 | 100 | Fondo | **TooCubeForest** |
| `ARCADECLASSIC.TTF` | fuente | — | Todos los textos | **Recursos** |

**Orden de dibujo** (de atrás hacia delante):
1. Fondo (−100)
2. Paredes y suelo (0)
3. Plataformas (1)
4. Puerta (5)
5. Caballero (20)
6. Lava (50-51)
7. HUD, siempre encima

---

## 8. Animaciones

### 8.1 Piezas
- **Hoja de sprites:** `caballero.png`, cortada en cuadros de 32 × 32 numerados desde 0.
- **4 clips** (`Art/Characters/Animaciones/`). Cada clip cambia el *Sprite* del SpriteRenderer a su ritmo:

| Clip | Cuadros | Velocidad | Bucle |
|---|---|---|---|
| `caballero_reposo` | 5, 6 | 4 por segundo | sí |
| `caballero_correr` | 0, 1, 2, 3 | 10 por segundo | sí |
| `caballero_saltar` | 4 | — | sí (1 cuadro) |
| `caballero_caer` | 7 | — | sí (1 cuadro) |

- **Controlador** `caballero.controller`, puesto en el Animator del objeto *Sprite* del Jugador:
  - **Estados:** Reposo (el inicial), Correr, Saltar y Caer.
  - **Parámetros:** `Velocidad` (float), `EnSuelo` (bool) y `VelocidadY` (float).
  - **Transiciones:** todas salen de **Any State**, con duración 0, sin *Exit Time* y sin volver a sí mismas.

| Va a | Condición |
|---|---|
| Reposo | EnSuelo y Velocidad < 0,1 |
| Correr | EnSuelo y Velocidad > 0,1 |
| Saltar | no EnSuelo y VelocidadY > 0 |
| Caer | no EnSuelo y VelocidadY < 0 |

### 8.2 Flujo en cada fotograma
```
PlayerController.Update
   1. lee controles, detecta el suelo, salta...
   2. UpdateAnimator():
         anim.SetFloat("Velocidad",  |velocidad x|)
         anim.SetBool ("EnSuelo",    IsGrounded)
         anim.SetFloat("VelocidadY", velocidad y)
Animator
   3. comprueba las 4 transiciones y elige el estado
   4. reproduce el clip, que cambia el Sprite del SpriteRenderer
```

**Ejemplo de un salto:**
1. Quieto → **Reposo**.
2. Pulsas D: Velocidad 7 → **Correr**.
3. Espacio: VelocidadY = 14,5, EnSuelo = no → **Saltar**.
4. Punto más alto: VelocidadY < 0 → **Caer**.
5. Aterriza: EnSuelo = sí → **Correr** o **Reposo**.

### 8.3 Efectos visuales que no son del Animator
| Efecto | Dónde | Cómo |
|---|---|---|
| Mirar a izquierda o derecha | PlayerController | `sprite.flipX = moveInput < 0` |
| Ponerse rojizo al perder | PlayerController.Die | `sprite.color = HurtColor` |
| Aviso naranja de la rompible | CrumblingPlatform | `part.color = warningColor` |
| Desaparecer y volver | CrumblingPlatform | `SpriteRenderer.enabled` y `collider.enabled` |
| Profundidad del fondo | CameraOnlyUp | paralaje (5.4) |

- **En pausa:** las animaciones se detienen solas, porque `Time.timeScale = 0`.
- **En la cuenta atrás:** el caballero queda en Reposo, porque no se mueve.

---

## 9. Interfaz (HUD)

- **Canvas** en modo *Screen Space – Overlay*, siempre encima.
- **Canvas Scaler** con resolución de referencia 1920 × 1080, para que se vea igual en cualquier pantalla.
- **Graphic Raycaster:** sin él, el ratón no podría pulsar los botones.

| Objeto | Componentes | Qué muestra |
|---|---|---|
| **Nivel** | Text + Outline | «EL PISO ES LAVA», arriba a la derecha (texto fijo) |
| **Mensaje** / **Submensaje** | Text + Outline | Cuenta atrás, «LA LAVA VA A SUBIR» y «YA!» |
| **Fondo barra lava** | Image | Rectángulo oscuro (decoración) |
| **Barra lava** | Image (*Filled*, vertical, desde abajo) | Se llena según la lava se acerca |
| **Panel** | Image (fondo oscuro translúcido) | Se muestra en pausa, al ganar y al perder |
| ↳ **Titulo / Cuerpo / Ayuda** | Text + Outline | Textos del panel |
| ↳ **Botones** | Vertical Layout Group | Coloca los botones en columna |
| ↳ **Boton Continuar / Repetir / Salir** | Image + Button + LayoutElement | Llaman a `Continue`, `Reload` y `Quit` del GameManager (eventos *On Click* del Inspector) |

- **Fuente:** ARCADECLASSIC. **Solo tiene letras, números y «!»**, así que las tildes, la «ñ» y signos como «¡» o «¿» no se ven.
- **Botones:** se iluminan en naranja al pasar el ratón o al elegirlos con teclado o mando.

---

## 10. Iluminación

El juego usa **URP 2D** (`Settings/UniversalRP.asset` con `Renderer2D.asset`), que permite luces 2D:

| Luz | Tipo | Color / intensidad / radio | Efecto |
|---|---|---|---|
| Luz global 2D | Global | blanca, 0,9 | Luz base de todo el nivel |
| Luz lava 1-3 | Punto | naranja, 1,4, radio 9 | La lava ilumina las paredes y plataformas cercanas mientras sube |
| Luz meta | Punto | verde claro, 1,2, radio 3 | La puerta brilla |

---

## 11. Una partida de principio a fin

1. **Carga de la escena:**
   - `GameManager.Start` pone la cuenta atrás en 3 y el estado en **Countdown**;
   - el HUD oculta el panel.
2. **Cuenta atrás (3 s):**
   - el HUD muestra 3, 2, 1 y «LA LAVA VA A SUBIR»;
   - el jugador no se puede mover y la lava está quieta.
3. **«YA!»:**
   - el estado pasa a **Playing**;
   - se guarda el momento en que empieza a subir la lava.
4. **Jugando:** en cada fotograma, el GameManager:
   - suma el tiempo;
   - sube la lava;
   - comprueba si la lava te alcanzó o si te caíste.

   Al mismo tiempo:
   - el jugador se mueve y el Animator cambia de clip;
   - la cámara sube y el fondo va detrás más despacio;
   - las rompibles reaccionan al pisarlas;
   - la barra de lava se actualiza.
5. **Final:**
   - **Puerta:** se llama a `PlayerReachedGoal`, que congela al jugador; el estado pasa a **Won** y aparece el panel con el tiempo.
   - **Lava o caída:** se llama a `Damage`, que congela al jugador y lo pone rojizo; el estado pasa a **Lost** y aparece el panel de derrota.
6. **Repetir** (botón o R) vuelve a cargar la escena; **Salir** cierra el juego.

### El tiempo de juego
- `ElapsedTime += Time.deltaTime` **solo en el estado Playing**, así que no cuenta la cuenta atrás ni la pausa, y se para al ganar o al perder.
- Al ganar se muestra redondeado hacia arriba: `Mathf.CeilToInt(ElapsedTime)`.

---

## 12. Dónde se cambia cada valor

Todo se cambia en el **Inspector**. Los cambios hechos **durante el Play no se guardan**: anota el valor, para el Play y ponlo de nuevo.

| Quieres cambiar… | Objeto | Campo (valor actual) |
|---|---|---|
| Cuenta atrás | GameManager | *Countdown* (3) |
| Velocidad de la lava | GameManager | *Base Speed* (0,45), *Acceleration* (0,05) |
| Ayuda / respiro de la lava | GameManager | *Max Gap* (10) × *Catch Up* (1,8); *Min Gap* (3) × *Mercy* (0,7) |
| Cuánto caer para perder | GameManager | *Fall Margin* (1,5) |
| Velocidad y salto del caballero | prefab Jugador → PlayerController | *Move Speed* (7), *Jump Height* (3,5), *Min Jump Height* (1,2), *Gravity* (30)… |
| Tiempo antes de que se rompa una plataforma | prefab PlataformaRompible | *Warning Time* (1) |
| Tiempo para que vuelva | prefab PlataformaRompible | *Respawn Time* (3) |
| Color del aviso | prefab PlataformaRompible | *Warning Color* |
| Seguimiento de la cámara | Main Camera → CameraOnlyUp | *Vertical Offset* (3), *Smooth Time* (0,25), *Min Y* (11,25), *Max Y* (148,75) |
| Paralaje del fondo | Main Camera → CameraOnlyUp | *Background Factor* (0,84) |
| Texto de la cuenta atrás | HUD | *Intro Text* |
| Sensibilidad de la barra de lava | HUD | *Lava Bar Distance* (15) |
| Textos de victoria y derrota | `HUD.cs` | líneas de `ShowPanel` en `OnStateChanged` |
| El nivel (plataformas, puerta, paredes) | Escena | mover o copiar los objetos (ver abajo) |

**Cambiar un valor en el código** (por ejemplo `public float acceleration = 0.25f;`) **no cambia el juego**: manda el valor guardado en la escena o el prefab, el que ves en el Inspector.

**Modificar el nivel:**
- arrastra los prefabs dentro de **Plataformas**;
- para el ancho, cambia *Size* del SpriteRenderer y el mismo tamaño en el BoxCollider2D;
- para hacerlo más largo: sube la **Meta**, estira las **paredes** y sube *Max Y* de la cámara;
- para que se pueda pasar: **no más de 3 filas** entre plataformas, ni **más de 2** si la de arriba es rompible.

---

## 13. Ajustes del proyecto

| Ajuste | Dónde | Valor |
|---|---|---|
| Escenas en la compilación | File → Build Profiles | solo `10_Nivel` |
| Nombre del juego | Project Settings → Player → Product Name | El Piso es Lava |
| Resolución por defecto | Project Settings → Player | 1920 × 1080, pantalla completa |
| Pipeline de render | Project Settings → Graphics / Quality | URP 2D (`UniversalRP`) |
| Controles | Project Settings → Player → Active Input Handling | *Both* (el juego solo usa el Input System; se podría poner *Input System Package (New)*) |
| Capas | Project Settings → Tags and Layers | Player 6, Ground 7, OneWay 8, Lava 12 |
| Etiquetas usadas | Tags | *Player* (jugador), *MainCamera* (cámara), *Finish* (meta) |
| Física 2D | Project Settings → Physics 2D | matriz de choques de la sección 6.1 |

**Respaldos** (fuera del proyecto): `proyecto u1 final\Respaldo videojuegoU1\`, con un zip por cada gran cambio.

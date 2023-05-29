using RimWorld.Planet;

namespace zed_0xff.YADA;

// prevent savegame loading error when previously saved with zed_0xff.YADA component
class Yada : global::YADA.Yada {
    public Yada(World w) : base(w) {
    }
}

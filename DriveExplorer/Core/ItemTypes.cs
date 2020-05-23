namespace DriveExplorer {
    public enum ItemTypes {
        Folder  = 0b000000001,
        Drive   = 0b000000010,
        File    = 0b000000100,
        IMG     = 0b000001000,
        TXT     = 0b000010000,
        DOC     = 0b000100000,
        XLS     = 0b001000000,
        PPT     = 0b010000000,
        ZIP     = 0b100000000,
        Folders = Drive | Folder,
        Files   = File | IMG | TXT | DOC | XLS | PPT | ZIP,
    }

    public static class ItemTypesExtensions {
        public static ItemTypes Add(this ItemTypes a, ItemTypes b) {
            return a | b;
        }
        public static ItemTypes Remove(this ItemTypes a, ItemTypes b) {
            return a & ~b;
        }
        public static bool Contains(this ItemTypes a, ItemTypes b) {
            return (a & b) == b;
        }
        public static bool Is(this ItemTypes a, ItemTypes b) {
            return (b & a) == a;
        }
    }
}

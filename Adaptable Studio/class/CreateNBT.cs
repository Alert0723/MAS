using fNbt;

namespace CommandStructureCreater
{
    public class CreateNBT
    {
        public static NbtList Blocks = new NbtList("blocks", NbtTagType.Compound);
        public static NbtList Palette = new NbtList("palette", NbtTagType.Compound);

        private static void CreatNbt(int i, int j, bool Portrait, string Command, bool Flat, short BlockType)
        {
            NbtCompound SubCompound = new NbtCompound();
            NbtList pos = new NbtList("pos");
            NbtCompound nbt = new NbtCompound("nbt");
            SubCompound.Clear();
            pos.Clear();
            nbt.Clear();

            if (i == 0 & j == 0) nbt.Add(new NbtByte("auto", 0)); else nbt.Add(new NbtByte("auto", 1));
            nbt.Add(new NbtByte("conditionMet", 0));
            nbt.Add(new NbtByte("powered", 0));
            nbt.Add(new NbtByte("TrackOutput", 1));
            nbt.Add(new NbtInt("SuccessCount", 0));
            nbt.Add(new NbtString("Command", Command));
            nbt.Add(new NbtString("CustomName", "@"));
            nbt.Add(new NbtString("id", "minecraft:command_block"));

            int x = 0; int y = 0; int z = 0;
            if (Portrait) { if (Flat) { x = j; z = i; } else { x = j; y = i; } }
            else { if (Flat) { x = i; z = j; } else { x = i; y = j; } }
            pos.Add(new NbtInt(x));
            pos.Add(new NbtInt(y));
            pos.Add(new NbtInt(z));

            SubCompound.Add(new NbtInt("state", BlockType));
            SubCompound.Add(pos);
            SubCompound.Add(nbt);
            Blocks.Add(SubCompound);
        }

        private static void CreateBlocks(long TotalCB, long Length, bool Portrait, string[] Commands, bool Flat)
        {
            int i = 0; int j = 0; int k = 0;
            while (k < TotalCB)
            {
                if (j % 2 == 0)
                {
                    if (i != Length - 1)
                    {
                        if (i == 0 & j == 0) { CreatNbt(i, j, Portrait, Commands[k], Flat, 0); }
                        else { CreatNbt(i, j, Portrait, Commands[k], Flat, 1); }
                        i++;
                    }
                    else
                    {
                        CreatNbt(i, j, Portrait, Commands[k], Flat, 2);
                        j++;
                    }
                }
                else
                {
                    if (i != 0)
                    {
                        CreatNbt(i, j, Portrait, Commands[k], Flat, 3);
                        i--;
                    }
                    else
                    {
                        CreatNbt(i, j, Portrait, Commands[k], Flat, 2);
                        j++;
                    }
                }
                k++;
            }
        }

        private static void CreatePalette(bool Portrait, bool Flat)
        {
            for (int i = 0; i < 4; i++)
            {
                //0:RCB    1:主方向CCB    2:换层CCB    3:反方向CCB
                NbtCompound SubCompound = new NbtCompound();
                NbtCompound Properties = new NbtCompound("Properties");
                SubCompound.Clear();
                Properties.Clear();
                Properties.Add(new NbtString("conditional", "false"));
                if (Portrait)
                {
                    if (Flat)
                    {
                        if (i < 2) { Properties.Add(new NbtString("facing", "south")); }
                        if (i == 2) { Properties.Add(new NbtString("facing", "east")); }
                        if (i == 3) { Properties.Add(new NbtString("facing", "north")); }
                    }
                    else
                    {
                        if (i < 2) { Properties.Add(new NbtString("facing", "up")); }
                        if (i == 2) { Properties.Add(new NbtString("facing", "east")); }
                        if (i == 3) { Properties.Add(new NbtString("facing", "down")); }
                    }
                }
                else
                {
                    if (Flat)
                    {
                        if (i < 2) { Properties.Add(new NbtString("facing", "east")); }
                        if (i == 2) { Properties.Add(new NbtString("facing", "south")); }
                        if (i == 3) { Properties.Add(new NbtString("facing", "west")); }
                    }
                    else
                    {
                        if (i < 2) { Properties.Add(new NbtString("facing", "east")); }
                        if (i == 2) { Properties.Add(new NbtString("facing", "up")); }
                        if (i == 3) { Properties.Add(new NbtString("facing", "west")); }
                    }
                }
                if (i != 0) { SubCompound.Add(new NbtString("Name", "minecraft:chain_command_block")); } else { SubCompound.Add(new NbtString("Name", "minecraft:repeating_command_block")); }
                SubCompound.Add(Properties);
                Palette.Add(SubCompound);
            }
        }

        public static void CreateStructure(long TotalCB, long Length, bool Portrait, string[] Commands, bool Flat, string Author, ref NbtCompound StructureNbt)
        {
            StructureNbt.Clear();
            StructureNbt.Add(new NbtInt("DataVersion", 922));
            StructureNbt.Add(new NbtString("author", Author));
            StructureNbt.Add(new NbtList("entities", NbtTagType.Compound));

            NbtList size = new NbtList("size");
            //for (int i = 0; i < 3; i++) { size.Add(new NbtInt(1)); }
            if (Flat) { size.Add(new NbtInt(2)); size.Add(new NbtInt(1)); size.Add(new NbtInt(2)); }
            else { size.Add(new NbtInt(2)); size.Add(new NbtInt(2)); size.Add(new NbtInt(1)); }
            StructureNbt.Add(size);

            CreatePalette(Portrait, Flat);
            StructureNbt.Add(Palette);
            CreateBlocks(TotalCB, Length, Portrait, Commands, Flat);
            StructureNbt.Add(Blocks);
        }
    }
}


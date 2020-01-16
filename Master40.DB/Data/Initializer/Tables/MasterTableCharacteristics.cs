using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using System.Collections.Generic;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableCharacteristic
    {
        //BasePlate
        internal M_Characteristic SideFront;
        internal M_Characteristic SideBack;
        internal M_Characteristic SideLeft;
        internal M_Characteristic SideRight;
        internal M_Characteristic Height;
        internal M_Characteristic Angle;
        internal M_Characteristic Recess;
        internal M_Characteristic Hole_Chassis_Front_Right;
        internal M_Characteristic Hole_Chassis_Front_Left;
        internal M_Characteristic Hole_Chassis_Back_Right;
        internal M_Characteristic Hole_Chassis_Back_Left;
        internal M_Characteristic Hole_Axle_Front;
        internal M_Characteristic Hole_Axle_Back;
        internal M_Characteristic Hole_Connector;
        //Dump-Truck
        internal M_Characteristic Clearance_Chassis;
        internal M_Characteristic Clearance_Truck_Bed;
        internal M_Characteristic Clearance_Ground;
        //Skeleton
        internal M_Characteristic Wheel_Front_Left;
        internal M_Characteristic Wheel_Front_Right;
        internal M_Characteristic Wheel_Back_Left;
        internal M_Characteristic Wheel_Back_Right;
        internal M_Characteristic Spacer_Position_Left;
        internal M_Characteristic Spacer_Position_Right;
        //Truck-Bed
        internal M_Characteristic Clearance_Tailboard_Right;
        internal M_Characteristic Clearance_Tailboard_Left;
        internal M_Characteristic Clearance_Tailboard_Bottom;
        //Chassis Dump-Truck
        internal M_Characteristic Difference_EngineBlock_Cabin;
        //Cabin
        internal M_Characteristic Cabin_SideFront;
        internal M_Characteristic Cabin_SideBack;
        internal M_Characteristic Cabin_SideRight;
        internal M_Characteristic Cabin_SideLeft;
        internal M_Characteristic Cabin_HeightFront;
        internal M_Characteristic Cabin_HeightBack;
        internal M_Characteristic Hole_Cabin_Front_Right;
        internal M_Characteristic Hole_Cabin_Front_Left;
        //Engine-Block
        internal M_Characteristic EngineBlock_SideFront;
        internal M_Characteristic EngineBlock_SideBack;
        internal M_Characteristic EngineBlock_SideRight;
        internal M_Characteristic EngineBlock_SideLeft;
        internal M_Characteristic EngineBlock_HeightBack;
        internal M_Characteristic Hole_EngineBlock_Front_Right;
        internal M_Characteristic Hole_EngineBlock_Front_Left;
        internal M_Characteristic Hole_EngineBlock_Back_Right;
        internal M_Characteristic Hole_EngineBlock_Back_Left;
        //Side Wall Long
        internal M_Characteristic WallLong_SideTop;
        internal M_Characteristic WallLong_SideBottom;
        internal M_Characteristic WallLong_SideFront;
        internal M_Characteristic WallLong_SideBack;
        internal M_Characteristic WallLong_Width;
        internal M_Characteristic WallLong_Angle;
        internal M_Characteristic Hole_WallLong_Front_Top;
        internal M_Characteristic Hole_WallLong_Front_Bottom;
        internal M_Characteristic Hole_WallLong_Bottom_Front;
        internal M_Characteristic Hole_WallLong_Bottom_Back;
        internal M_Characteristic Hole_WallLong_Back_Top;
        //Side Wall Short
        internal M_Characteristic WallShort_SideTop;
        internal M_Characteristic WallShort_SideBottom;
        internal M_Characteristic WallShort_SideRight;
        internal M_Characteristic WallShort_SideLeft;
        internal M_Characteristic WallShort_Length;
        internal M_Characteristic WallShort_Angle;
        internal M_Characteristic Hole_WallShort_Right_Top;
        internal M_Characteristic Hole_WallShort_Right_Middle;
        internal M_Characteristic Hole_WallShort_Right_Bottom;
        internal M_Characteristic Hole_WallShort_Left_Top;
        internal M_Characteristic Hole_WallShort_Left_Middle;
        internal M_Characteristic Hole_WallShort_Left_Bottom;
        //BasePlate Truck-Bed
        internal M_Characteristic BasePlateTB_SideFront;
        internal M_Characteristic BasePlateTB_SideBack;
        internal M_Characteristic BasePlateTB_SideRight;
        internal M_Characteristic BasePlateTB_SideLeft;
        internal M_Characteristic BasePlateTB_Height;
        internal M_Characteristic BasePlateTB_Angle;
        internal M_Characteristic Hole_BasePlateTB_Front_Right;
        internal M_Characteristic Hole_BasePlateTB_Front_Left;
        internal M_Characteristic Hole_BasePlateTB_Back_Right;
        internal M_Characteristic Hole_BasePlateTB_Back_Left;
        internal M_Characteristic Hole_BasePlateTB_Bottom_Right;
        internal M_Characteristic Hole_BasePlateTB_Bottom_Left;

        internal MasterTableCharacteristic(MasterTableArticle articles, MasterTableOperation operations)
        {
            SideFront = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_CUT.Id,IsReadOnly = false, Name = "Length Front" };
            SideBack = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_CUT.Id, IsReadOnly = false, Name = "Length Back" };
            SideLeft = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_CUT.Id, IsReadOnly = false, Name = "Length Left Side" };
            SideRight = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_CUT.Id, IsReadOnly = false, Name = "Length Right Side" };
            Height = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_CUT.Id, IsReadOnly = false, Name = "Height" };
            Angle = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_CUT.Id, IsReadOnly = false, Name = "Angle Between Two Sides" };
            Recess = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_CUT.Id, IsReadOnly = false, Name = "Recess" };
            Hole_Chassis_Front_Right = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_DRILL.Id, IsReadOnly = false, Name = "Hole Chassis Front Right" };
            Hole_Chassis_Front_Left = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_DRILL.Id, IsReadOnly = false, Name = "Hole Chassis Front Left" };
            Hole_Chassis_Back_Right = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_DRILL.Id, IsReadOnly = false, Name = "Hole Chassis Back Right" };
            Hole_Chassis_Back_Left = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_DRILL.Id, IsReadOnly = false, Name = "Hole Chassis Back Left" };
            Hole_Axle_Front = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_DRILL.Id, IsReadOnly = false, Name = "Hole Axle Front" };
            Hole_Axle_Back = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_DRILL.Id, IsReadOnly = false, Name = "Hole Axle Back" };
            Hole_Connector = new M_Characteristic { ArticleId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_DRILL.Id, IsReadOnly = false, Name = "Hole Connector" };
            Clearance_Chassis = new M_Characteristic { ArticleId = articles.DUMP_TRUCK.Id, OperationId = operations.DUMP_TRUCK_WEDDING.Id, IsReadOnly = false, Name = "Clearance Chassis to Truck-Bed" };
            Clearance_Ground = new M_Characteristic { ArticleId = articles.DUMP_TRUCK.Id, OperationId = operations.DUMP_TRUCK_WEDDING.Id, IsReadOnly = false, Name = "Clearance Baseplate to Ground" };
            Clearance_Truck_Bed = new M_Characteristic { ArticleId = articles.DUMP_TRUCK.Id, OperationId = operations.DUMP_TRUCK_WEDDING.Id, IsReadOnly = false, Name = "Clearance Truck-Bed to Baseplate" };
            Wheel_Front_Left = new M_Characteristic { ArticleId = articles.SKELETON.Id, OperationId = operations.SCREW_WHEELS.Id, IsReadOnly = false, Name = "Distance Wheel Front Left" };
            Wheel_Front_Right = new M_Characteristic { ArticleId = articles.SKELETON.Id, OperationId = operations.SCREW_WHEELS.Id, IsReadOnly = false, Name = "Distance Wheel Front Right" };
            Wheel_Back_Left = new M_Characteristic { ArticleId = articles.SKELETON.Id, OperationId = operations.SCREW_WHEELS.Id, IsReadOnly = false, Name = "Distance Wheel Back Left" };
            Wheel_Back_Right = new M_Characteristic { ArticleId = articles.SKELETON.Id, OperationId = operations.SCREW_WHEELS.Id, IsReadOnly = false, Name = "Distance Wheel Back Right" };
            Spacer_Position_Left = new M_Characteristic { ArticleId = articles.SKELETON.Id, OperationId = operations.GLUE_SEMITRAILER.Id, IsReadOnly = false, Name = "Spacer Position Left" };
            Spacer_Position_Right = new M_Characteristic { ArticleId = articles.SKELETON.Id, OperationId = operations.GLUE_SEMITRAILER.Id, IsReadOnly = false, Name = "Spacer Position Right" };
            Clearance_Tailboard_Right = new M_Characteristic { ArticleId = articles.TRUCK_BED.Id, OperationId = operations.GLUE_TRUCK_BED.Id, IsReadOnly = false, Name = "Clearance Right Side Tailboard" };
            Clearance_Tailboard_Left = new M_Characteristic { ArticleId = articles.TRUCK_BED.Id, OperationId = operations.GLUE_TRUCK_BED.Id, IsReadOnly = false, Name = "Clearance Left Side Tailboard" };
            Clearance_Tailboard_Bottom = new M_Characteristic { ArticleId = articles.TRUCK_BED.Id, OperationId = operations.GLUE_TRUCK_BED.Id, IsReadOnly = false, Name = "Clearance Bottom Side Tailboard" };
            Difference_EngineBlock_Cabin = new M_Characteristic { ArticleId = articles.CHASSIS_TYPE_DUMP.Id, OperationId = operations.DUMP_TRUCK_MOUNT_ENGINE.Id, IsReadOnly = false, Name = "Planeness between Engineblock and Cabin" };
            Cabin_SideFront = new M_Characteristic { ArticleId = articles.CABIN.Id, OperationId = operations.CABIN_CUT.Id, IsReadOnly = false, Name = "Length Front" };
            Cabin_SideBack = new M_Characteristic { ArticleId = articles.CABIN.Id, OperationId = operations.CABIN_CUT.Id, IsReadOnly = false, Name = "Length Back" };
            Cabin_SideRight = new M_Characteristic { ArticleId = articles.CABIN.Id, OperationId = operations.CABIN_CUT.Id, IsReadOnly = false, Name = "Length Right" };
            Cabin_SideLeft = new M_Characteristic { ArticleId = articles.CABIN.Id, OperationId = operations.CABIN_CUT.Id, IsReadOnly = false, Name = "Length Left" };
            Cabin_HeightFront = new M_Characteristic { ArticleId = articles.CABIN.Id, OperationId = operations.CABIN_CUT.Id, IsReadOnly = false, Name = "Height Front" };
            Cabin_HeightBack = new M_Characteristic { ArticleId = articles.CABIN.Id, OperationId = operations.CABIN_CUT.Id, IsReadOnly = false, Name = "Height Back" };
            Hole_Cabin_Front_Right = new M_Characteristic { ArticleId = articles.CABIN.Id, OperationId = operations.CABIN_DRILL.Id, IsReadOnly = false, Name = "Hole Cabin Front Right" };
            Hole_Cabin_Front_Left = new M_Characteristic { ArticleId = articles.CABIN.Id, OperationId = operations.CABIN_DRILL.Id, IsReadOnly = false, Name = "Hole Cabin Front Left" };
            EngineBlock_SideFront = new M_Characteristic { ArticleId = articles.ENGINE_BLOCK.Id, OperationId = operations.ENGINE_BLOCK_CUT.Id, IsReadOnly = false, Name = "Length Front" };
            EngineBlock_SideBack = new M_Characteristic { ArticleId = articles.ENGINE_BLOCK.Id, OperationId = operations.ENGINE_BLOCK_CUT.Id, IsReadOnly = false, Name = "Length Back" };
            EngineBlock_SideRight = new M_Characteristic { ArticleId = articles.ENGINE_BLOCK.Id, OperationId = operations.ENGINE_BLOCK_CUT.Id, IsReadOnly = false, Name = "Length Right" };
            EngineBlock_SideLeft = new M_Characteristic { ArticleId = articles.ENGINE_BLOCK.Id, OperationId = operations.ENGINE_BLOCK_CUT.Id, IsReadOnly = false, Name = "Length Left" };
            EngineBlock_HeightBack = new M_Characteristic { ArticleId = articles.ENGINE_BLOCK.Id, OperationId = operations.ENGINE_BLOCK_CUT.Id, IsReadOnly = false, Name = "Height Back" };
            Hole_EngineBlock_Front_Right = new M_Characteristic { ArticleId = articles.ENGINE_BLOCK.Id, OperationId = operations.ENGINE_BLOCK_DRILL.Id, IsReadOnly = false, Name = "Hole Engine-Block Front Right" };
            Hole_EngineBlock_Front_Left = new M_Characteristic { ArticleId = articles.ENGINE_BLOCK.Id, OperationId = operations.ENGINE_BLOCK_DRILL.Id, IsReadOnly = false, Name = "Hole Engine-Block Front Left" };
            Hole_EngineBlock_Back_Right = new M_Characteristic { ArticleId = articles.ENGINE_BLOCK.Id, OperationId = operations.ENGINE_BLOCK_DRILL.Id, IsReadOnly = false, Name = "Hole Engine-Block Back Right" };
            Hole_EngineBlock_Back_Left = new M_Characteristic { ArticleId = articles.ENGINE_BLOCK.Id, OperationId = operations.ENGINE_BLOCK_DRILL.Id, IsReadOnly = false, Name = "Hole Engine-Block Back Left" };
            WallLong_SideTop = new M_Characteristic { ArticleId = articles.SIDEWALL_LONG.Id, OperationId = operations.SIDEWALL_LONG_CUT.Id, IsReadOnly = false, Name = "Length Top" };
            WallLong_SideBottom = new M_Characteristic { ArticleId = articles.SIDEWALL_LONG.Id, OperationId = operations.SIDEWALL_LONG_CUT.Id, IsReadOnly = false, Name = "Length Bottom" };
            WallLong_SideFront = new M_Characteristic { ArticleId = articles.SIDEWALL_LONG.Id, OperationId = operations.SIDEWALL_LONG_CUT.Id, IsReadOnly = false, Name = "Length Front" };
            WallLong_SideBack = new M_Characteristic { ArticleId = articles.SIDEWALL_LONG.Id, OperationId = operations.SIDEWALL_LONG_CUT.Id, IsReadOnly = false, Name = "Length Back" };
            WallLong_Width = new M_Characteristic { ArticleId = articles.SIDEWALL_LONG.Id, OperationId = operations.SIDEWALL_LONG_CUT.Id, IsReadOnly = false, Name = "Width" };
            WallLong_Angle = new M_Characteristic { ArticleId = articles.SIDEWALL_LONG.Id, OperationId = operations.SIDEWALL_LONG_CUT.Id, IsReadOnly = false, Name = "Angle Between Two Sides" };
            Hole_WallLong_Front_Top = new M_Characteristic { ArticleId = articles.SIDEWALL_LONG.Id, OperationId = operations.SIDEWALL_LONG_DRILL.Id, IsReadOnly = false, Name = "Hole Front Top" };
            Hole_WallLong_Front_Bottom = new M_Characteristic { ArticleId = articles.SIDEWALL_LONG.Id, OperationId = operations.SIDEWALL_LONG_DRILL.Id, IsReadOnly = false, Name = "Hole Front Bottom" };
            Hole_WallLong_Bottom_Front = new M_Characteristic { ArticleId = articles.SIDEWALL_LONG.Id, OperationId = operations.SIDEWALL_LONG_DRILL.Id, IsReadOnly = false, Name = "Hole Bottom Front" };
            Hole_WallLong_Bottom_Back = new M_Characteristic { ArticleId = articles.SIDEWALL_LONG.Id, OperationId = operations.SIDEWALL_LONG_DRILL.Id, IsReadOnly = false, Name = "Hole Bottom Back" };
            Hole_WallLong_Back_Top = new M_Characteristic { ArticleId = articles.SIDEWALL_LONG.Id, OperationId = operations.SIDEWALL_LONG_DRILL.Id, IsReadOnly = false, Name = "Hole Back Top" };
            WallShort_SideTop = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_CUT.Id, IsReadOnly = false, Name = "Width Top" };
            WallShort_SideBottom = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_CUT.Id, IsReadOnly = false, Name = "Width Bottom" };
            WallShort_SideRight = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_CUT.Id, IsReadOnly = false, Name = "Width Right" };
            WallShort_SideLeft = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_CUT.Id, IsReadOnly = false, Name = "Width Left" };
            WallShort_Length = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_CUT.Id, IsReadOnly = false, Name = "Length" };
            WallShort_Angle = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_CUT.Id, IsReadOnly = false, Name = "Angle Between Two Sides" };
            Hole_WallShort_Right_Top = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_DRILL.Id, IsReadOnly = false, Name = "Hole Right Top" };
            Hole_WallShort_Right_Middle = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_DRILL.Id, IsReadOnly = false, Name = "Hole Right Middle" };
            Hole_WallShort_Right_Bottom = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_DRILL.Id, IsReadOnly = false, Name = "Hole Right Bottom" };
            Hole_WallShort_Left_Top = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_DRILL.Id, IsReadOnly = false, Name = "Hole Left Top" };
            Hole_WallShort_Left_Middle = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_DRILL.Id, IsReadOnly = false, Name = "Hole Left Middle" };
            Hole_WallShort_Left_Bottom = new M_Characteristic { ArticleId = articles.SIDEWALL_SHORT.Id, OperationId = operations.SIDEWALL_SHORT_DRILL.Id, IsReadOnly = false, Name = "Hole Left Bottom" };
            BasePlateTB_SideFront = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_CUT.Id, IsReadOnly = false, Name = "Width Front" };
            BasePlateTB_SideBack = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_CUT.Id, IsReadOnly = false, Name = "Width Back" };
            BasePlateTB_SideRight = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_CUT.Id, IsReadOnly = false, Name = "Length Right" };
            BasePlateTB_SideLeft = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_CUT.Id, IsReadOnly = false, Name = "Length Left" };
            BasePlateTB_Height = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_CUT.Id, IsReadOnly = false, Name = "Height" };
            BasePlateTB_Angle = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_CUT.Id, IsReadOnly = false, Name = "Angle Between Two Sides" };
            Hole_BasePlateTB_Front_Right = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_DRILL.Id, IsReadOnly = false, Name = "Hole Front Right" };
            Hole_BasePlateTB_Front_Left = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_DRILL.Id, IsReadOnly = false, Name = "Hole Front Left" };
            Hole_BasePlateTB_Back_Right = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_DRILL.Id, IsReadOnly = false, Name = "Hole Back Right" };
            Hole_BasePlateTB_Back_Left = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_DRILL.Id, IsReadOnly = false, Name = "Hole Back Left" };
            Hole_BasePlateTB_Bottom_Right = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_DRILL.Id, IsReadOnly = false, Name = "Hole Bottom Right" };
            Hole_BasePlateTB_Bottom_Left = new M_Characteristic { ArticleId = articles.BASEPLATE_TRUCK_BED.Id, OperationId = operations.BASEPLATE_TRUCK_BED_DRILL.Id, IsReadOnly = false, Name = "Hole Bottom Left" };
        }
        internal M_Characteristic[] Init(MasterDBContext context)
        {                                       
            // Units
            var characteristics = new M_Characteristic[]
            {
                SideFront,
                SideBack,
                SideLeft,
                SideRight,
                Height,
                Angle,
                Recess,
                Hole_Chassis_Front_Right,
                Hole_Chassis_Front_Left,
                Hole_Chassis_Back_Right,
                Hole_Chassis_Back_Left,
                Hole_Axle_Front,
                Hole_Axle_Back,
                Hole_Connector,
                Clearance_Chassis,
                Clearance_Ground,
                Clearance_Truck_Bed,
                Wheel_Front_Left,
                Wheel_Front_Right,
                Wheel_Back_Right,
                Wheel_Back_Left,
                Spacer_Position_Right,
                Spacer_Position_Left,
                Clearance_Tailboard_Right,
                Clearance_Tailboard_Left,
                Clearance_Tailboard_Bottom,
                Difference_EngineBlock_Cabin,
                Cabin_SideFront,
                Cabin_SideBack,
                Cabin_SideRight,
                Cabin_SideLeft,
                Cabin_HeightFront,
                Cabin_HeightBack,
                Hole_Cabin_Front_Right,
                Hole_Cabin_Front_Left,
                EngineBlock_SideFront,
                EngineBlock_SideBack,
                EngineBlock_SideRight,
                EngineBlock_SideLeft,
                EngineBlock_HeightBack,
                Hole_EngineBlock_Front_Right,
                Hole_EngineBlock_Front_Left,
                Hole_EngineBlock_Back_Right,
                Hole_EngineBlock_Back_Left,
                WallLong_SideTop,
                WallLong_SideBottom,
                WallLong_SideFront,
                WallLong_SideBack,
                WallLong_Width,
                WallLong_Angle,
                Hole_WallLong_Front_Top,
                Hole_WallLong_Front_Bottom,
                Hole_WallLong_Bottom_Front,
                Hole_WallLong_Bottom_Back,
                Hole_WallLong_Back_Top,
                WallShort_SideTop,
                WallShort_SideBottom,
                WallShort_SideRight,
                WallShort_SideLeft,
                WallShort_Length,
                WallShort_Angle,
                Hole_WallShort_Right_Top,
                Hole_WallShort_Right_Middle,
                Hole_WallShort_Right_Bottom,
                Hole_WallShort_Left_Top,
                Hole_WallShort_Left_Middle,
                Hole_WallShort_Left_Bottom,
                BasePlateTB_SideFront,
                BasePlateTB_SideBack,
                BasePlateTB_SideRight,
                BasePlateTB_SideLeft,
                BasePlateTB_Height,
                BasePlateTB_Angle,
                Hole_BasePlateTB_Front_Right,
                Hole_BasePlateTB_Front_Left,
                Hole_BasePlateTB_Back_Right,
                Hole_BasePlateTB_Back_Left,
                Hole_BasePlateTB_Bottom_Right,
                Hole_BasePlateTB_Bottom_Left
        };
            context.Characteristics.AddRange(entities: characteristics);
            context.SaveChanges();
            return characteristics;
        }
    }
}

using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Master40.DB.Nominal;
using Zpp.Util.Graph.impl;

namespace Zpp.Util.Graph
{
    public interface IScheduleNode: IId
    {
        DueTime GetEndTimeBackward();

        DueTime GetStartTimeBackward();

        /**
         * Adapts the startTime and also adapts the endTime accordingly (if exists)
         */
        void SetStartTimeBackward(DueTime startTime);

        /**
         * Adapts the endTime and also adapts the startTime accordingly (if exists)
         */
        void SetEndTimeBackward(DueTime endTime);

        /**
         * Contains transition time if exits
         */
        Duration GetDuration();

        void SetFinished();

        void SetInProgress();

        bool IsFinished();

        NodeType GetNodeType();

        void ClearStartTimeBackward();

        void ClearEndTimeBackward();

        State? GetState();

        /**
         * There comes a time, when an entity is finished e.g. ProductionOrder is finished producing
         * --> entity is not allowed to change anymore regarding time/amount
         * OR an initial StockExchangeDemand that simulates the initial stock is not allowed to change in time
         */
        void SetReadOnly();

        bool IsReadOnly();
    }
}
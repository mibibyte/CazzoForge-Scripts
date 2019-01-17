using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using InfServer.Logic;
using InfServer.Game;
using InfServer.Scripting;
using InfServer.Bots;
using InfServer.Protocol;

using Assets;

namespace InfServer.Script.GameType_Multi
{
    public class Boundary
    {
        public short _top;
        public short _bottom;
        public short _left;
        public short _right;
        private int tickLastWave;
        private Arena _arena;

        private bool _bShrink;
        private bool _bShrinkVertical;
        private int _shrinkAmount;
        private int _lastShrink;


        public short Height
        {
            get
            {
                return (short)(_bottom - _top);
            }
        }

        public short Width
        {
            get
            {
                return (short)(_right - _left);
            }
        }

        public Boundary(Arena arena, short top, short bottom, short right, short left)
        {
            _arena = arena;
            _top = top;
            _bottom = bottom;
            _left = left;
            _right = right;

        }

        public void Shrink(short shrinkAmount, bool bShrinkVertical)
        {
            _shrinkAmount = shrinkAmount;
            _bShrinkVertical = bShrinkVertical;
            _bShrink = true;
            _arena.sendArenaMessage("Play area is shrinking, Don't get caught in the storm!");
        }

        public void Poll(int now)
        {

            if (_bShrink && now - _lastShrink >= 250)
            {
                if (_shrinkAmount <= 0)
                {
                    _bShrink = false;
                    _bShrinkVertical = false;
                    _shrinkAmount = 0;
                }

                if (_bShrinkVertical)
                {
                    _top += 40;
                    _bottom -= 40;
                    _left += 40;
                    _right -= 40;

                    _shrinkAmount -= 160;
                }
                else
                {
                    _left += 40;
                    _right -= 40;

                    _shrinkAmount -= 80;
                }

                _lastShrink = now;
            }

            if (now - tickLastWave >= 1000)
            {
                Helpers.ObjectState state = new Helpers.ObjectState();
                Helpers.ObjectState target = new Helpers.ObjectState();

                state.positionX = _left;
                state.positionY = _top;
                target.positionX = _right;
                target.positionY = _top;

                byte fireAngle = Helpers.computeLeadFireAngle(state, target, 8000 / 1000);
                Helpers.Player_RouteExplosion(_arena.Players, 1452, _left, _top, 0, fireAngle, 0);

                state.positionX = _right;
                state.positionY = _top;
                target.positionX = _right;
                target.positionY = _bottom;

                fireAngle = Helpers.computeLeadFireAngle(state, target, 8000 / 1000);
                Helpers.Player_RouteExplosion(_arena.Players, 1452, _right, _top, 0, fireAngle, 0);

                state.positionX = _right;
                state.positionY = _bottom;
                target.positionX = _left;
                target.positionY = _bottom;

                fireAngle = Helpers.computeLeadFireAngle(state, target, 8000 / 1000);
                Helpers.Player_RouteExplosion(_arena.Players, 1452, _right, _bottom, 0, fireAngle, 0);


                state.positionX = _left;
                state.positionY = _bottom;
                target.positionX = _left;
                target.positionY = _top;

                fireAngle = Helpers.computeLeadFireAngle(state, target, 8000 / 1000);
                Helpers.Player_RouteExplosion(_arena.Players, 1452, _left, _bottom, 0, fireAngle, 0);

                tickLastWave = now;
            }
        }
    }
}

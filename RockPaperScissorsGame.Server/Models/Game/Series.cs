using System;
using System.Collections.Generic;
using RockPaperScissorsGame.Common;
using RockPaperScissorsGame.Server.Exceptions;

namespace RockPaperScissorsGame.Server.Models.Game
{
    public class Series
    {
        public readonly string Player1;
        public string Player2 { get; set; }
        
        private readonly List<OnlineRound> _rounds;
        private OnlineRound _activeRound;
        
        public Series(string player1Id)
        {
            Player1 = player1Id;
            _rounds = new List<OnlineRound>();
        }

        public void CreateNewRound()
        {
            OnlineRound newRound = new OnlineRound(Player1, Player2);
            _rounds.Add(newRound);
            _activeRound = newRound;
        }
        
        public RoundMove Player(string userId)
        {
            if (_activeRound.Player1.PlayerId.Equals(userId))
            {
                return _activeRound.Player1;
            }
            if (_activeRound.Player2.PlayerId.Equals(userId))
            {
                return _activeRound.Player2;
            }

            return null;
            // throw new ServiceException("Access violated");
        }

        public RoundMove Opponent(string userId)
        {
            if (_activeRound.Player1.PlayerId.Equals(userId))
            {
                return _activeRound.Player2;
            }
            if (_activeRound.Player2.PlayerId.Equals(userId))
            {
                return _activeRound.Player1;
            }
            
            return null;
            //throw new ServiceException("Access violated");
        }
        
        public void MakeMove(string userId, MoveOptions figure)
        {
            Player(userId).SelectedOption = figure;
        }
        
    }
}

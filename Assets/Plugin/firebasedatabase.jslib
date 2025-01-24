mergeInto(LibraryManager.library, {

    getData: function(_address) {
        var address = UTF8ToString(_address);
         _wall = address;
        try {

             firebase.database().ref(`idle-game/${address}`).once('value').then(function(snapshot) {
               if(snapshot.val()){
                    console.log(snapshot.val());
                    
                    window._unityInstance.SendMessage("Menu", "OnGetDataFB", JSON.stringify(snapshot.val()));
                }else{
                    console.log('register'+ address);
                    
                    window._unityInstance.SendMessage("Menu", "OnGetDataFB", JSON.stringify(register(address)));
                }
            });

        } catch (error) {
           return 'error';
        }
    },

    updgradeDamage: function(_address, _skillID, _damage) {
        var address = UTF8ToString(_address);
        var skillID = _skillID;
        var damage = _damage;
        console.log("upgrade damage execute")
        try {
           
            firebase.database().ref(`idle-game/${address}/skills/${skillID}/damageLevel`).set(damage).then(function(unused) {
                console.log(_damage);
            });

        } catch (error) {
            console.log(error);
        }
    },

    updgradeSpeed: function(_address, _skillID, _speed) {
        var address = UTF8ToString(_address);
        var skillID = _skillID;
        var speed = _speed;
        console.log("upgrade speed execute")
        try {

            firebase.database().ref(`idle-game/${address}/skills/${skillID}/speedLevel`).set(speed).then(function(unused) {
                console.log(_speed);
            });

        } catch (error) {
           console.log(error);
        }
    },

    unlockSkill: function(_address, _skillID) {
        var address = UTF8ToString(_address);
        var skillID = _skillID;

        try {

             firebase.database().ref(`idle-game/${address}/skills/${skillID}/unlocked`).set(true).then(function(unused) {
                console.log(skillID)
            });

        } catch (error) {
          console.log("error")
        }
    },

    changeActiveSkill: function(_address, _skillID) {
        var address = UTF8ToString(_address);
        var skillID = _skillID;

        try {

             firebase.database().ref(`idle-game/${address}/activatedSkillID`).set(_skillID).then(function(unused) {
                console.log(_skillID);
            });

        } catch (error) {
           console.log("error")
        }
    },

    nextLevel: function(_address, _level) {
        var address = UTF8ToString(_address);
        var level = _level;

        try {

            firebase.database().ref(`idle-game/${address}/level`).set(level).then(function(unused) {
                console.log(level);
            });

        } catch (error) {
           console.log("error")
        }
    },

    getTempMoney: function(_address, _tempMoney) {
        var address = UTF8ToString(_address);
        var tempMoney = _tempMoney;
        console.log("getTempMoney");
        try {

            firebase.database().ref(`idle-game/${address}/tempMoney`).set(tempMoney).then(function(unused) {
                console.log(tempMoney);
            });

        } catch (error) {
           console.log("error")
        }
    },

    getLeaderboard: function() {
        try {
            console.log("load leaderboard js");

            // Retrieve all records without limit
            firebase.database().ref('idle-game')
                .orderByChild('level')   // Ascending order by 'level'
                .once('value', function(snapshot) {
                    var _leaderboard = {};
                    snapshot.forEach(function(childSnapshot) {
                        var userId = childSnapshot.key;
                        var userData = childSnapshot.val();
                        _leaderboard[userId] = userData;
                    });

                    // Sort the leaderboard by level in descending order
                    var sortedLeaderboard = Object.keys(_leaderboard).sort(function(a, b) {
                        return _leaderboard[b].level - _leaderboard[a].level; // Descending order
                    }).reduce(function(sortedObj, key) {
                        sortedObj[key] = _leaderboard[key];
                        return sortedObj;
                    }, {});

                    // Extract the top 10 players after sorting
                    var top10Leaderboard = Object.keys(sortedLeaderboard).slice(0, 10).reduce(function(obj, key) {
                        obj[key] = sortedLeaderboard[key];
                        return obj;
                    }, {});

                    console.log("Top 10 Leaderboard: ", top10Leaderboard);

                    // Send the top 10 leaderboard to Unity
                    window._unityInstance.SendMessage("GameManager", "OnGetLeaderboard", JSON.stringify(top10Leaderboard));
                });

        } catch (error) {
            console.error('Error getting leaderboard:', error);
            return 'error';
        }
    }

});
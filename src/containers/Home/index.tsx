import React, { useContext, useEffect, useState } from 'react'
import authentication from 'react-azure-adb2c';
import { AuthContext } from 'containers/Auth/B2CAuth';

import LeaderboardTable from 'components/LeaderboardTable/LeaderboardTable';

import Paper from '@material-ui/core/Paper';

const Home = (): JSX.Element => {

    const [users, setUsers] = useState([])

    const signOut = () => {
        console.log('auth: ', authentication.getAccessToken());
        authentication.signOut();
    }

    useEffect(() => {
        const token = authentication.getAccessToken();
        console.log(token);
        fetch('https://sswconsulting-dev.azurewebsites.net/api/leaderboard/get',{
            headers:{
                'Content-Type': 'application/json',
            Accept: 'application/json',
            Authorization: `Bearer ${token}`,
            }
            
        }).then(e => {
            e.json().then(d => {
                console.log(d);
                setUsers(d.users);
            })
        }).catch(e => {
            console.error(e);
        })
    }, [])

    return (
        <div>
        <Paper>
            <LeaderboardTable users={users}></LeaderboardTable>
            <p>
                <button onClick={signOut}>Sign Out</button>
            </p>
        </Paper>
        </div>
    )
}

export default Home

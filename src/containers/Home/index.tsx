import React, { useEffect, useState } from 'react'
import authentication from 'react-azure-adb2c';
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
            credentials: 'include',
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
        <Paper>
            <LeaderboardTable users={users}></LeaderboardTable>
            <p>
                <button onClick={signOut}>Sign Out</button>
            </p>
        </Paper>
    )
}

export default Home;
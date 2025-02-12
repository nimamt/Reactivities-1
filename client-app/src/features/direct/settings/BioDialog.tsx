import { useEffect, useState } from 'react';
import Dialog from '@mui/material/Dialog/Dialog';
import Paper from '@mui/material/Paper/Paper';
import { observer } from 'mobx-react-lite';
import { useStore } from '../../../app/stores/store';
import React from 'react';
import Slide from '@mui/material/Slide/Slide';
import { TransitionProps } from '@mui/material/transitions';
import IconButton from '@mui/material/IconButton/IconButton';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import CloseIcon from '@mui/icons-material/Close';
import DoneIcon from '@mui/icons-material/Done';
import Typography from '@mui/material/Typography/Typography';
import Stack from '@mui/material/Stack/Stack';
import Input from '@mui/material/Input/Input';

export interface Props {
    open: boolean;
    onClose: () => void;
}

const Transition = React.forwardRef(function Transition(
    props: TransitionProps & {
        children: React.ReactElement<any, any>;
    },
    ref: React.Ref<unknown>
) {
    return <Slide direction="left" ref={ref} {...props} />;
});

export default observer(function BioDialog({ open, onClose }: Props) {
    const {
        settingsStore: { profile, updateBio }
    } = useStore();

    const handleClose = () => {
        onClose();
    };

    const [value, setValue] = useState(profile && profile.bio ? profile.bio : '');

    useEffect(() => {
        setValue(profile && profile.bio ? profile.bio : '');
    }, [profile]);

    return (
        <Dialog onClose={handleClose} open={open} TransitionComponent={Transition}>
            <AppBar position="relative" elevation={1}>
                <Toolbar variant="dense">
                    <IconButton size="large" edge="start" color="inherit" aria-label="menu" sx={{ mr: 2 }} onClick={handleClose}>
                        <CloseIcon fontSize="large" />
                    </IconButton>
                    <Typography variant="h6">Edit Bio</Typography>
                    <div style={{ flex: '1' }} />
                    <IconButton
                        onClick={() => {
                            updateBio(value);
                            handleClose();
                        }}
                    >
                        <DoneIcon />
                    </IconButton>
                </Toolbar>
            </AppBar>
            <Paper sx={{ minWidth: '30vw' }}>
                <div style={{ padding: '1rem' }}>
                    <Stack>
                        <Input
                            placeholder="Bio (optional)"
                            value={value}
                            multiline
                            onChange={(e) => setValue(e.target.value)}
                            sx={{
                                fontSize: '1.6rem',
                                padding: 1.5,
                                paddingLeft: 2.5
                            }}
                        />
                        <Typography sx={{ marginTop: '1rem' }}>You can add a few lines about yourself. Anyone who opens your profile will see this text.</Typography>
                    </Stack>
                </div>
            </Paper>
        </Dialog>
    );
});

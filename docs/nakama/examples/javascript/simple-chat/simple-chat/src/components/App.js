import React from 'react';
import {
  Container,
  Grid,
  Menu,
  Segment
} from 'semantic-ui-react';

import Chat from './Chat';

export default () => {
  return (
    <Segment vertical>
      <Menu size='huge' fixed='top' inverted borderless>
        <Container fluid>
          <Menu.Item header>Simple chat with Nakama server</Menu.Item>
        </Container>
      </Menu>

      <Grid padded divided>
        <Grid.Row columns={2}>
          <Grid.Column>
            <Chat />
          </Grid.Column>
          <Grid.Column>
            <Chat />
          </Grid.Column>
        </Grid.Row>
      </Grid>
    </Segment>
  );
};
